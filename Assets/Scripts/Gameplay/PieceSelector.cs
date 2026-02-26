using System.Collections.Generic;
using UnityEngine;

namespace ComBoom.Gameplay
{
    public struct SelectionContext
    {
        public float fillRatio;
        public int turnsWithoutClear;
        public int comboCount;
        public int setsWithoutSmall;
    }

    public static class PieceSelector
    {
        public static PieceData[] SelectSet(SelectionContext ctx, GameGrid grid)
        {
            var allPieces = PieceDatabase.AllPieces;

            // 1) Build candidate list (exclude oversized pieces when board is very full)
            var candidates = new List<PieceData>();
            bool veryFull = ctx.fillRatio > 0.80f;
            foreach (var p in allPieces)
            {
                if (veryFull && (p.name == "Square3x3" || p.blockCount >= 5))
                    continue;
                candidates.Add(p);
            }

            // Fallback: if all were excluded, use small pieces
            if (candidates.Count == 0)
                candidates = PieceDatabase.GetPiecesBySize(PieceSize.Small);

            // 2) Compute base category weights from fill ratio
            float wSmall, wMedium, wLarge;
            if (ctx.fillRatio > 0.70f)
            {
                wSmall = 5f; wMedium = 3f; wLarge = 1f;
            }
            else if (ctx.fillRatio < 0.30f)
            {
                wSmall = 1f; wMedium = 3f; wLarge = 5f;
            }
            else
            {
                wSmall = 2f; wMedium = 3f; wLarge = 2f;
            }

            // 3) Mercy modifier: 3+ turns without clear AND >60% full
            bool mercyActive = ctx.turnsWithoutClear >= 3 && ctx.fillRatio > 0.60f;
            if (mercyActive)
            {
                wSmall *= 3f;
                wLarge *= 0.3f;
            }

            // 4) Build per-piece weights
            var weights = new float[candidates.Count];
            for (int i = 0; i < candidates.Count; i++)
            {
                PieceSize size = PieceDatabase.GetPieceSize(candidates[i]);
                float w = size == PieceSize.Small ? wSmall
                        : size == PieceSize.Medium ? wMedium
                        : wLarge;

                // Combo bonus: line pieces get extra weight
                if (ctx.comboCount >= 2 && candidates[i].name.Contains("Line"))
                {
                    w *= 1f + ctx.comboCount * 0.5f;
                }

                // Combo bonus: pieces that fit somewhere on the board get extra weight
                if (ctx.comboCount >= 2 && grid != null && grid.CanPlaceAny(candidates[i]))
                {
                    w *= 1.3f;
                }

                weights[i] = w;
            }

            // 5) Select 3 pieces with set rules
            var result = new PieceData[3];
            int largeCount = 0;

            for (int slot = 0; slot < 3; slot++)
            {
                // Mercy: last slot guaranteed Small
                if (mercyActive && slot == 2)
                {
                    result[slot] = PickFromCategory(PieceSize.Small, candidates, weights);
                    continue;
                }

                PieceData picked = WeightedPick(candidates, weights);

                // Max 2 Large per set
                if (PieceDatabase.GetPieceSize(picked) == PieceSize.Large)
                {
                    largeCount++;
                    if (largeCount > 2)
                    {
                        picked = PickFromCategory(PieceSize.Medium, candidates, weights);
                        if (picked == null)
                            picked = PickFromCategory(PieceSize.Small, candidates, weights);
                    }
                }

                result[slot] = picked;
            }

            // 6) Bad luck protection: 3+ consecutive sets without Small → force one Small
            if (ctx.setsWithoutSmall >= 3)
            {
                bool hasSmall = false;
                for (int i = 0; i < 3; i++)
                {
                    if (PieceDatabase.GetPieceSize(result[i]) == PieceSize.Small)
                    {
                        hasSmall = true;
                        break;
                    }
                }

                if (!hasSmall)
                {
                    result[2] = PickFromCategory(PieceSize.Small, candidates, weights);
                }
            }

            return result;
        }

        private static PieceData WeightedPick(List<PieceData> candidates, float[] weights)
        {
            float total = 0f;
            for (int i = 0; i < weights.Length; i++)
                total += weights[i];

            if (total <= 0f)
                return candidates[Random.Range(0, candidates.Count)];

            float roll = Random.Range(0f, total);
            float cumulative = 0f;
            for (int i = 0; i < candidates.Count; i++)
            {
                cumulative += weights[i];
                if (roll <= cumulative)
                    return candidates[i];
            }

            return candidates[candidates.Count - 1];
        }

        private static PieceData PickFromCategory(PieceSize size, List<PieceData> candidates, float[] weights)
        {
            // Build sub-list of matching candidates
            float total = 0f;
            var indices = new List<int>();
            for (int i = 0; i < candidates.Count; i++)
            {
                if (PieceDatabase.GetPieceSize(candidates[i]) == size)
                {
                    indices.Add(i);
                    total += weights[i];
                }
            }

            if (indices.Count == 0)
            {
                // Fallback: pick from all small pieces in database
                var fallback = PieceDatabase.GetPiecesBySize(size);
                if (fallback.Count > 0)
                    return fallback[Random.Range(0, fallback.Count)];
                return candidates[Random.Range(0, candidates.Count)];
            }

            if (total <= 0f)
                return candidates[indices[Random.Range(0, indices.Count)]];

            float roll = Random.Range(0f, total);
            float cumulative = 0f;
            for (int i = 0; i < indices.Count; i++)
            {
                cumulative += weights[indices[i]];
                if (roll <= cumulative)
                    return candidates[indices[i]];
            }

            return candidates[indices[indices.Count - 1]];
        }
    }
}
