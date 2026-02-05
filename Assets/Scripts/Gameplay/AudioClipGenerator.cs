using UnityEngine;

namespace ComBoom.Gameplay
{
    public static class AudioClipGenerator
    {
        private const int SampleRate = 44100;

        /// <summary>Short ascending chirp for piece pick-up.</summary>
        public static AudioClip CreatePickClip()
        {
            int samples = (int)(SampleRate * 0.08f);
            float[] data = new float[samples];
            float phase = 0f;
            for (int i = 0; i < samples; i++)
            {
                float progress = (float)i / samples;
                float env = (1f - progress) * (1f - progress);
                float freq = Mathf.Lerp(700f, 1400f, progress);
                phase += 2f * Mathf.PI * freq / SampleRate;
                data[i] = Mathf.Sin(phase) * env * 0.35f;
            }
            return CreateClip("Pick", data);
        }

        /// <summary>Satisfying snap/thud for piece placement.</summary>
        public static AudioClip CreatePlaceClip()
        {
            int samples = (int)(SampleRate * 0.12f);
            float[] data = new float[samples];
            float phaseLow = 0f;
            float phaseHi = 0f;
            float phaseClick = 0f;
            for (int i = 0; i < samples; i++)
            {
                float progress = (float)i / samples;
                float env = Mathf.Exp(-progress * 8f);

                phaseLow += 2f * Mathf.PI * 200f / SampleRate;
                phaseHi += 2f * Mathf.PI * 400f / SampleRate;
                phaseClick += 2f * Mathf.PI * 2200f / SampleRate;

                float body = Mathf.Sin(phaseLow) + Mathf.Sin(phaseHi) * 0.4f;
                float click = (progress < 0.06f) ? Mathf.Sin(phaseClick) * (1f - progress / 0.06f) : 0f;
                data[i] = (body + click) * env * 0.3f;
            }
            return CreateClip("Place", data);
        }

        /// <summary>Multi-layered line clear: chime sweep + sparkle + sub-bass thump.</summary>
        public static AudioClip CreateClearClip()
        {
            int samples = (int)(SampleRate * 0.35f);
            float[] data = new float[samples];
            float phaseChime = 0f, phaseFifth = 0f;
            float phaseSparkle = 0f;
            float phaseBass = 0f;
            for (int i = 0; i < samples; i++)
            {
                float progress = (float)i / samples;

                // Envelope: quick attack, smooth decay
                float env;
                if (progress < 0.04f)
                    env = progress / 0.04f;
                else
                    env = Mathf.Exp(-(progress - 0.04f) * 3f);

                // Layer 1: Chime sweep C5→G5 with fifth
                float chimeFreq = Mathf.Lerp(523f, 784f, progress);
                float fifthFreq = Mathf.Lerp(659f, 988f, progress);
                phaseChime += 2f * Mathf.PI * chimeFreq / SampleRate;
                phaseFifth += 2f * Mathf.PI * fifthFreq / SampleRate;
                float chime = Mathf.Sin(phaseChime) * 0.25f + Mathf.Sin(phaseFifth) * 0.15f;

                // Layer 2: High sparkle with tremolo
                float sparkleFreq = Mathf.Lerp(2500f, 5000f, progress);
                phaseSparkle += 2f * Mathf.PI * sparkleFreq / SampleRate;
                float tremolo = 0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * 30f * progress);
                float sparkle = Mathf.Sin(phaseSparkle) * 0.06f * tremolo * env;

                // Layer 3: Sub-bass thump
                phaseBass += 2f * Mathf.PI * 100f / SampleRate;
                float bassEnv = Mathf.Exp(-progress * 18f);
                float bass = Mathf.Sin(phaseBass) * 0.15f * bassEnv;

                data[i] = (chime * env + sparkle + bass);
            }
            return CreateClip("Clear", data);
        }

        /// <summary>Rich ascending arpeggio C5-E5-G5-B5-C6 with harmonics and chorus.</summary>
        public static AudioClip CreateComboClip()
        {
            int samples = (int)(SampleRate * 0.5f);
            float[] data = new float[samples];
            float[] notes = { 523.25f, 659.25f, 783.99f, 987.77f, 1046.50f };
            float phase = 0f, phaseThird = 0f, phaseChorus = 0f;
            for (int i = 0; i < samples; i++)
            {
                float progress = (float)i / samples;
                int noteIdx = Mathf.Min((int)(progress * notes.Length), notes.Length - 1);
                float noteProgress = (progress * notes.Length) - noteIdx;

                // Per-note envelope: attack + decay
                float env;
                if (noteProgress < 0.05f)
                    env = noteProgress / 0.05f;
                else
                    env = Mathf.Exp(-(noteProgress - 0.05f) * 4f);

                // Volume ramp: each note slightly louder
                float volRamp = 0.7f + 0.3f * ((float)noteIdx / (notes.Length - 1));

                float freq = notes[noteIdx];
                phase += 2f * Mathf.PI * freq / SampleRate;
                phaseThird += 2f * Mathf.PI * freq * 3f / SampleRate;
                phaseChorus += 2f * Mathf.PI * (freq + 2f) / SampleRate;

                float fundamental = Mathf.Sin(phase);
                float harmonic = Mathf.Sin(phaseThird) * 0.15f;
                float chorus = Mathf.Sin(phaseChorus) * 0.12f;

                data[i] = (fundamental + harmonic + chorus) * env * volRamp * 0.25f;
            }
            return CreateClip("Combo", data);
        }

        /// <summary>Descending sad tones (G4-Eb4-C4) for game over.</summary>
        public static AudioClip CreateGameOverClip()
        {
            int samples = (int)(SampleRate * 0.7f);
            float[] data = new float[samples];
            float[] notes = { 392f, 311.13f, 261.63f };
            float phase = 0f;
            float phaseOvertone = 0f;
            for (int i = 0; i < samples; i++)
            {
                float progress = (float)i / samples;
                int noteIdx = Mathf.Min((int)(progress * notes.Length), notes.Length - 1);
                float noteProgress = (progress * notes.Length) - noteIdx;
                float env = Mathf.Exp(-noteProgress * 2.5f) * (1f - progress * 0.4f);

                phase += 2f * Mathf.PI * notes[noteIdx] / SampleRate;
                phaseOvertone += 2f * Mathf.PI * notes[noteIdx] * 2f / SampleRate;

                float main = Mathf.Sin(phase);
                float overtone = Mathf.Sin(phaseOvertone) * 0.15f;
                data[i] = (main + overtone) * env * 0.3f;
            }
            return CreateClip("GameOver", data);
        }

        /// <summary>Short UI click for buttons and toggles.</summary>
        public static AudioClip CreateClickClip()
        {
            int samples = (int)(SampleRate * 0.04f);
            float[] data = new float[samples];
            float phase = 0f;
            for (int i = 0; i < samples; i++)
            {
                float progress = (float)i / samples;
                float env = (1f - progress) * (1f - progress) * (1f - progress);
                phase += 2f * Mathf.PI * 1000f / SampleRate;
                data[i] = Mathf.Sin(phase) * env * 0.35f;
            }
            return CreateClip("Click", data);
        }

        /// <summary>Ambient music loop: Am-F-C-G chord progression, ~27 seconds.</summary>
        public static AudioClip CreateMusicLoop()
        {
            float bpm = 72f;
            float beatDur = 60f / bpm;
            int bars = 8;
            int beatsPerBar = 4;
            float totalDuration = bars * beatsPerBar * beatDur;
            int samples = (int)(SampleRate * totalDuration);
            float[] data = new float[samples];

            // Chord progression: Am-F-C-G (2 bars each)
            float[][] chords = {
                new float[] { 220f, 261.63f, 329.63f },  // Am: A3, C4, E4
                new float[] { 174.61f, 220f, 261.63f },  // F: F3, A3, C4
                new float[] { 130.81f, 164.81f, 196f },  // C: C3, E3, G3
                new float[] { 98f, 123.47f, 146.83f }    // G: G2, B2, D3
            };
            float[] bassNotes = { 110f, 87.31f, 65.41f, 49f }; // Bass: A2, F2, C2, G1

            float barDur = beatsPerBar * beatDur;
            float[] padPhases = new float[3];
            float bassPhase = 0f;
            float arpPhase = 0f;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float progress = (float)i / samples;

                // Determine current chord (each chord = 2 bars)
                int chordIdx = Mathf.Min((int)(t / (barDur * 2f)), chords.Length - 1);
                float chordTime = t - chordIdx * barDur * 2f;
                float[] chord = chords[chordIdx];

                // Pad layer: soft sine chords with slow envelope
                float padSample = 0f;
                for (int n = 0; n < 3; n++)
                {
                    padPhases[n] += 2f * Mathf.PI * chord[n] / SampleRate;
                    // Smooth transition envelope
                    float chordProgress = chordTime / (barDur * 2f);
                    float padEnv = 1f;
                    if (chordProgress < 0.05f) padEnv = chordProgress / 0.05f;
                    else if (chordProgress > 0.92f) padEnv = (1f - chordProgress) / 0.08f;
                    // Mix sine + slight triangle for warmth
                    float sine = Mathf.Sin(padPhases[n]);
                    padSample += sine * padEnv * 0.07f;
                }

                // Bass layer: root note on beats 1 and 3
                float beatPos = (t % barDur) / beatDur;
                float beatFrac = beatPos - Mathf.Floor(beatPos);
                int beatNum = (int)beatPos;
                float bassSample = 0f;
                if (beatNum == 0 || beatNum == 2)
                {
                    float bassEnv = Mathf.Exp(-beatFrac * 3f);
                    bassPhase += 2f * Mathf.PI * bassNotes[chordIdx] / SampleRate;
                    bassSample = Mathf.Sin(bassPhase) * bassEnv * 0.10f;
                }
                else
                {
                    bassPhase += 2f * Mathf.PI * bassNotes[chordIdx] / SampleRate;
                }

                // Arpeggio layer: cycle chord tones every 16th note
                float sixteenthDur = beatDur * 0.25f;
                float sixteenthPos = t % sixteenthDur;
                int arpNoteIdx = ((int)(t / sixteenthDur)) % 3;
                float arpEnv = Mathf.Exp(-(sixteenthPos / sixteenthDur) * 4f);
                arpPhase += 2f * Mathf.PI * chord[arpNoteIdx] * 2f / SampleRate; // octave up
                float arpSample = Mathf.Sin(arpPhase) * arpEnv * 0.03f;

                data[i] = padSample + bassSample + arpSample;
            }

            // Crossfade last 0.5s with first 0.5s for seamless loop
            int fadeSamples = (int)(SampleRate * 0.5f);
            for (int i = 0; i < fadeSamples; i++)
            {
                float fadeOut = (float)i / fadeSamples;
                float fadeIn = 1f - fadeOut;
                int endIdx = samples - fadeSamples + i;
                data[endIdx] = data[endIdx] * fadeIn + data[i] * fadeOut;
            }

            return CreateClip("MusicLoop", data);
        }

        private static AudioClip CreateClip(string name, float[] data)
        {
            AudioClip clip = AudioClip.Create(name, data.Length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
