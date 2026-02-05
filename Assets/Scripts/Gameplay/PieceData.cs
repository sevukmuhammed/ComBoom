using UnityEngine;

namespace ComBoom.Gameplay
{
    [System.Serializable]
    public class PieceData
    {
        public string name;
        public bool[,] shape;
        public Color color;
        public int blockCount;

        public PieceData(string name, bool[,] shape)
        {
            this.name = name;
            this.shape = shape;
            this.color = Color.white;
            this.blockCount = CountBlocks();
        }

        private int CountBlocks()
        {
            int count = 0;
            for (int r = 0; r < shape.GetLength(0); r++)
                for (int c = 0; c < shape.GetLength(1); c++)
                    if (shape[r, c]) count++;
            return count;
        }

        public int Rows => shape.GetLength(0);
        public int Cols => shape.GetLength(1);
    }
}
