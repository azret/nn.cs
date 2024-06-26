﻿namespace nn {
    using System;

    public interface IRNG {
        uint randint32();
        float randfloat32();
        ulong randint64();
        double randfloat64();
    }

    public static unsafe class rand {

        /// <summary>
        // A Mersenne Twister pseudorandom number generator. Copyright(c) Makoto Matsumoto and Takuji Nishimura.
        /// </summary>
        public class mt19937 : IRNG {

            static readonly uint[] MATRIX_A = { 0x0u, 0x9908b0df };

            const uint LMASK = 0x7fffffff;
            const uint UMASK = 0x80000000;

            const int MERSENNE_STATE_M = 397;
            const int MERSENNE_STATE_N = 624;

            uint[] state_;

            int left_;
            int next_;

            public mt19937(uint seed = 5489) {
                state_ = new uint[MERSENNE_STATE_N];
                left_ = 1;
                next_ = 0;
                init_with_uint32(seed);
            }

            void init_with_uint32(uint seed) {
                unchecked {
                    state_ = new uint[MERSENNE_STATE_N];
                    state_[0] = seed & 0xffffffff;
                    for (uint j = 1; j < MERSENNE_STATE_N; j++) {
                        state_[j] = 1812433253 * (state_[j - 1] ^ (state_[j - 1] >> 30)) + j;
                        state_[j] &= 0xffffffff;
                    }
                }
                left_ = 1;
                next_ = 0;
            }

            void next_state() {
                left_ = MERSENNE_STATE_N;
                next_ = 0;
                uint y, j;
                unchecked {
                    for (j = 0; j < MERSENNE_STATE_N - MERSENNE_STATE_M; j++) {
                        y = (state_[j] & UMASK) | (state_[j + 1] & LMASK);
                        state_[j] = state_[j + MERSENNE_STATE_M] ^ (y >> 1) ^ MATRIX_A[y & 0x1];
                    }
                    for (; j < MERSENNE_STATE_N - 1; j++) {
                        y = (state_[j] & UMASK) | (state_[j + 1] & LMASK);
                        state_[j] = state_[j + (MERSENNE_STATE_M - MERSENNE_STATE_N)] ^ (y >> 1) ^ MATRIX_A[y & 0x1];
                    }
                    y = (state_[MERSENNE_STATE_N - 1] & UMASK) | (state_[0] & LMASK);
                    state_[MERSENNE_STATE_N - 1] = state_[MERSENNE_STATE_M - 1] ^ (y >> 1) ^ MATRIX_A[y & 0x1];
                }
            }

            public uint randint32() {
                if (state_ == null) init_with_uint32(5489);
                if (--left_ <= 0) {
                    next_state();
                }
                uint y = state_[next_++];
                unchecked {
                    y ^= y >> 11;
                    y ^= (y << 7) & 0x9d2c5680;
                    y ^= (y << 15) & 0xefc60000;
                    y ^= y >> 18;
                }
                return y;
            }

            public ulong randint64() {
                return ((ulong)randint32() << 32) | randint32();
            }

            public float randfloat32() {
                return (randint32() & ((1ul << 24) - 1)) * (1.0f / (1ul << 24));
            }

            public double randfloat64() {
                return (randint64() & ((1ul << 53) - 1)) * (1.0d / (1ul << 53));
            }
        }

        /// <summary>
        /// The 31-bit multiplicative congruential pseudorandom number generator MCG(1132489760, 2^31 -1) [L'Ecuyer99]
        /// </summary>
        public class mcg31m1 {
            ulong state_;
            public mcg31m1(uint seed = 1) {
                state_ = seed % 0x000000007FFFFFFF;
            }

            public uint randint32() {
                uint x = (uint)(state_ % 0x000000007FFFFFFF);
                unchecked {
                    state_ = (1132489760 * state_) % 0x000000007FFFFFFF;
                }
                return x;
            }
            public ulong randint64() { return ((ulong)randint32() << 32) | randint32(); }
            public float randfloat32() { return (float)randint32() / 0x7FFFFFFF; }
            public double randfloat64() { return (double)randint32() / 0x7FFFFFFF; }
        }

        // Uniform Distribution

        public static float uniform(IRNG g, float from = 0f, float to = 1f) { return (float)g.randfloat32() * (to - from) + from; }
        public static double uniform(IRNG g, double from = 0f, double to = 1f) { return (double)g.randfloat64() * (to - from) + from; }

        public static void uniform_(float[] data, IRNG g, float from = 0f, float to = 1f) { fixed (float* ptr = data) { uniform_(ptr, (uint)data.Length, g, from, to); } }
        public static void uniform_(float* data, uint numel, IRNG g, float from = 0f, float to = 1f) { for (uint t = 0; t < numel; t++) { data[t] = uniform(g, from , to); } }

        // Normal Distribution

        public static void normal_(float[] data, IRNG g, float mean = 0f, float std = 1f) { fixed (float* ptr = data) { normal_(ptr, (uint)data.Length, g, mean, std); } }
        public static void normal_(float* data, uint numel, IRNG g, float mean = 0f, float std = 1f) {
            // This implementation follows PyTorch so that we are numerically identical when running verification tests.
            const double EPSILONE = 1e-12;
            if (numel >= 16) {
                // for (uint t = 0; t < numel; t++) {
                //     data[t] = uniform(g, 0f, 1f);
                // }
                uniform_(data, numel, g, 0f, 1f);
                for (uint i = 0; i < numel - 15; i += 16) {
                    _fill_16(data + i, mean, std);
                }
                if (numel % 16 != 0) {
                    // recompute the last 16 values
                    data = data + numel - 16;
                    // for (uint i = 0; i < 16; i++) {
                    //     data[i] = uniform(g, 0f, 1f);
                    // }
                    uniform_(data, 16, g, 0f, 1f);
                    _fill_16(data, mean, std);
                }
                void _fill_16(float* data, float mean, float std) {
                    for (uint t = 0; t < 8; t++) {
                        var u1 = 1 - data[t];
                        var u2 = data[t + 8];
                        var radius = Math.Sqrt(-2 * Math.Log(u1 + EPSILONE));
                        var theta = 2.0 * Math.PI * u2;
                        data[t] = (float)(radius * Math.Cos(theta) * std + mean);
                        data[t + 8] = (float)(radius * Math.Sin(theta) * std + mean);
                    }
                }
            } else {
                double? next_double_normal_sample = null;
                for (uint t = 0; t < numel; t++) {
                    if (next_double_normal_sample.HasValue) {
                        data[t] = (float)(next_double_normal_sample.Value * std + mean);
                        next_double_normal_sample = null;
                        continue;
                    }
                    // for numel < 16 we draw a double (float64)
                    var u1 = uniform(g, 0d, 1d);
                    var u2 = uniform(g, 0d, 1d);
                    var radius = Math.Sqrt(-2 * Math.Log(1 - u2 + EPSILONE));
                    var theta = 2.0 * Math.PI * u1;
                    next_double_normal_sample = radius * Math.Sin(theta);
                    data[t] = (float)(radius * Math.Cos(theta) * std + mean);
                }
            }
        }
    }
}
