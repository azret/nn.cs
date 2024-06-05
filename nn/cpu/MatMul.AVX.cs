﻿namespace nn.CPU {
    using System;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    /// <summary>
    /// Vectorized "C" implementation of MatMul (AVX)
    /// </summary>
    public class MatMulAVX : MatMulC {
        static byte[] matmul_backward = new byte[] {
            0x48, 0x8B, 0xC4, 0x4C, 0x89, 0x48, 0x20, 0x4C, 0x89, 0x40, 0x18, 0x48, 0x89, 0x50, 0x10, 0x48,
            0x83, 0xEC, 0x48, 0x4C, 0x8B, 0xD2, 0x4D, 0x8B, 0xD8, 0x33, 0xD2, 0x89, 0x14, 0x24, 0x39, 0x94,
            0x24, 0x90, 0x00, 0x00, 0x00, 0x0F, 0x86, 0xEA, 0x02, 0x00, 0x00, 0x4C, 0x8B, 0x84, 0x24, 0x88,
            0x00, 0x00, 0x00, 0x48, 0x89, 0x58, 0x08, 0x8B, 0x9C, 0x24, 0x98, 0x00, 0x00, 0x00, 0x48, 0x89,
            0x68, 0xF8, 0x48, 0x89, 0x70, 0xF0, 0x48, 0x89, 0x78, 0xE8, 0x4C, 0x89, 0x60, 0xE0, 0x44, 0x8B,
            0xA4, 0x24, 0xA0, 0x00, 0x00, 0x00, 0x4C, 0x89, 0x68, 0xD8, 0x4C, 0x89, 0x70, 0xD0, 0x4C, 0x89,
            0x78, 0xC8, 0x0F, 0x1F, 0x40, 0x00, 0x66, 0x66, 0x0F, 0x1F, 0x84, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x8B, 0xC2, 0x45, 0x33, 0xFF, 0x0F, 0xAF, 0xC3, 0x4D, 0x8D, 0x34, 0x83, 0x4D, 0x8D, 0x0C, 0x81,
            0x45, 0x85, 0xE4, 0x0F, 0x84, 0x4D, 0x02, 0x00, 0x00, 0x8B, 0xC2, 0x4D, 0x8B, 0xE8, 0x41, 0x0F,
            0xAF, 0xC4, 0x89, 0x44, 0x24, 0x04, 0x66, 0x66, 0x0F, 0x1F, 0x84, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x41, 0x03, 0xC7, 0x41, 0x8B, 0xCF, 0x0F, 0xAF, 0xCB, 0x33, 0xD2, 0x45, 0x33, 0xDB, 0xC4, 0xC1,
            0x7A, 0x10, 0x14, 0x82, 0x48, 0x8B, 0x44, 0x24, 0x70, 0xC5, 0xF8, 0x28, 0xDA, 0xC5, 0xE8, 0xC6,
            0xDA, 0x00, 0x4C, 0x8D, 0x14, 0x88, 0x48, 0x8B, 0x44, 0x24, 0x78, 0x4C, 0x8D, 0x04, 0x88, 0x85,
            0xDB, 0x0F, 0x84, 0xC5, 0x01, 0x00, 0x00, 0x83, 0xFB, 0x04, 0x0F, 0x82, 0x8D, 0x00, 0x00, 0x00,
            0x8D, 0x43, 0xFF, 0x48, 0x63, 0xC8, 0x49, 0x8D, 0x34, 0x8E, 0x49, 0x8D, 0x04, 0x88, 0x4C, 0x3B,
            0xC6, 0x77, 0x05, 0x49, 0x3B, 0xC6, 0x73, 0x75, 0x49, 0x8D, 0x3C, 0x89, 0x4C, 0x3B, 0xC7, 0x77,
            0x05, 0x49, 0x3B, 0xC1, 0x73, 0x67, 0x49, 0x8D, 0x0C, 0x8A, 0x4C, 0x3B, 0xC1, 0x77, 0x05, 0x49,
            0x3B, 0xC2, 0x73, 0x59, 0x4C, 0x3B, 0xCE, 0x77, 0x05, 0x49, 0x3B, 0xFE, 0x73, 0x4F, 0x4C, 0x3B,
            0xC9, 0x77, 0x05, 0x49, 0x3B, 0xFA, 0x73, 0x45, 0x8B, 0xFB, 0x83, 0xE7, 0xFC, 0x49, 0x8B, 0xF2,
            0x49, 0x8B, 0xEE, 0x49, 0x2B, 0xF1, 0x49, 0x2B, 0xE9, 0x49, 0x8B, 0xC8, 0x49, 0x8B, 0xC1, 0x49,
            0x2B, 0xC9, 0xC5, 0xE0, 0x59, 0x0C, 0x06, 0xC5, 0xF0, 0x58, 0x08, 0xC5, 0xF8, 0x11, 0x08, 0xC5,
            0xE0, 0x59, 0x0C, 0x28, 0xC5, 0xF0, 0x58, 0x0C, 0x01, 0x83, 0xC2, 0x04, 0x49, 0x83, 0xC3, 0x04,
            0xC5, 0xF8, 0x11, 0x0C, 0x01, 0x48, 0x8D, 0x40, 0x10, 0x3B, 0xD7, 0x72, 0xD5, 0x3B, 0xD3, 0x0F,
            0x83, 0x27, 0x01, 0x00, 0x00, 0x8B, 0xC3, 0x2B, 0xC2, 0x83, 0xF8, 0x04, 0x0F, 0x82, 0xD6, 0x00,
            0x00, 0x00, 0x8B, 0xC3, 0x49, 0x8D, 0x4B, 0x01, 0x2B, 0xC2, 0x49, 0x8D, 0x0C, 0x89, 0x83, 0xE8,
            0x04, 0x49, 0x8B, 0xF8, 0xC1, 0xE8, 0x02, 0x49, 0x8B, 0xF2, 0x49, 0x8B, 0xEE, 0x49, 0x2B, 0xF9,
            0x49, 0x2B, 0xF1, 0x49, 0x2B, 0xE9, 0xFF, 0xC0, 0x44, 0x8B, 0xE0, 0x8D, 0x14, 0x82, 0x4D, 0x8D,
            0x1C, 0x83, 0x0F, 0x1F, 0x40, 0x00, 0x66, 0x66, 0x0F, 0x1F, 0x84, 0x00, 0x00, 0x00, 0x00, 0x00,
            0xC5, 0xEA, 0x59, 0x44, 0x31, 0xFC, 0xC5, 0xFA, 0x58, 0x49, 0xFC, 0xC5, 0xFA, 0x11, 0x49, 0xFC,
            0xC5, 0xEA, 0x59, 0x44, 0x29, 0xFC, 0xC5, 0xFA, 0x58, 0x4C, 0x0F, 0xFC, 0xC5, 0xFA, 0x11, 0x4C,
            0x0F, 0xFC, 0xC5, 0xEA, 0x59, 0x04, 0x31, 0xC5, 0xFA, 0x58, 0x09, 0xC5, 0xFA, 0x11, 0x09, 0xC5,
            0xEA, 0x59, 0x04, 0x29, 0xC5, 0xFA, 0x58, 0x0C, 0x0F, 0xC5, 0xFA, 0x11, 0x0C, 0x0F, 0xC5, 0xEA,
            0x59, 0x44, 0x31, 0x04, 0xC5, 0xFA, 0x58, 0x49, 0x04, 0xC5, 0xFA, 0x11, 0x49, 0x04, 0xC5, 0xEA,
            0x59, 0x44, 0x29, 0x04, 0xC5, 0xFA, 0x58, 0x4C, 0x0F, 0x04, 0xC5, 0xFA, 0x11, 0x4C, 0x0F, 0x04,
            0xC5, 0xEA, 0x59, 0x44, 0x31, 0x08, 0xC5, 0xFA, 0x58, 0x49, 0x08, 0xC5, 0xFA, 0x11, 0x49, 0x08,
            0xC5, 0xEA, 0x59, 0x44, 0x29, 0x08, 0xC5, 0xFA, 0x58, 0x4C, 0x0F, 0x08, 0xC5, 0xFA, 0x11, 0x4C,
            0x0F, 0x08, 0x48, 0x8D, 0x49, 0x10, 0x49, 0x83, 0xEC, 0x01, 0x0F, 0x85, 0x70, 0xFF, 0xFF, 0xFF,
            0x44, 0x8B, 0xA4, 0x24, 0xA0, 0x00, 0x00, 0x00, 0x3B, 0xD3, 0x73, 0x40, 0x4B, 0x8D, 0x0C, 0x99,
            0x4D, 0x2B, 0xD1, 0x4D, 0x8B, 0xDE, 0x4D, 0x2B, 0xC1, 0x4D, 0x2B, 0xD9, 0x8B, 0xC3, 0x2B, 0xC2,
            0x8B, 0xD0, 0xC4, 0xA1, 0x6A, 0x59, 0x04, 0x11, 0xC5, 0xFA, 0x58, 0x09, 0xC5, 0xFA, 0x11, 0x09,
            0xC4, 0xC1, 0x6A, 0x59, 0x04, 0x0B, 0xC4, 0xC1, 0x7A, 0x58, 0x0C, 0x08, 0xC4, 0xC1, 0x7A, 0x11,
            0x0C, 0x08, 0x48, 0x8D, 0x49, 0x04, 0x48, 0x83, 0xEA, 0x01, 0x75, 0xD6, 0x4C, 0x8B, 0x84, 0x24,
            0x88, 0x00, 0x00, 0x00, 0x4D, 0x85, 0xC0, 0x74, 0x0C, 0xC4, 0xC1, 0x6A, 0x58, 0x45, 0x00, 0xC4,
            0xC1, 0x7A, 0x11, 0x45, 0x00, 0x8B, 0x44, 0x24, 0x04, 0x41, 0xFF, 0xC7, 0x4C, 0x8B, 0x54, 0x24,
            0x58, 0x49, 0x83, 0xC5, 0x04, 0x45, 0x3B, 0xFC, 0x0F, 0x82, 0xD2, 0xFD, 0xFF, 0xFF, 0x8B, 0x14,
            0x24, 0x4C, 0x8B, 0x5C, 0x24, 0x60, 0x4C, 0x8B, 0x4C, 0x24, 0x68, 0xFF, 0xC2, 0x89, 0x14, 0x24,
            0x3B, 0x94, 0x24, 0x90, 0x00, 0x00, 0x00, 0x0F, 0x82, 0x83, 0xFD, 0xFF, 0xFF, 0x4C, 0x8B, 0x7C,
            0x24, 0x10, 0x4C, 0x8B, 0x74, 0x24, 0x18, 0x4C, 0x8B, 0x6C, 0x24, 0x20, 0x4C, 0x8B, 0x64, 0x24,
            0x28, 0x48, 0x8B, 0x7C, 0x24, 0x30, 0x48, 0x8B, 0x74, 0x24, 0x38, 0x48, 0x8B, 0x6C, 0x24, 0x40,
            0x48, 0x8B, 0x5C, 0x24, 0x50, 0x48, 0x83, 0xC4, 0x48, 0xC3
        };

        static byte[] matmul_forward_kernel = {
            0x48, 0x89, 0x74, 0x24, 0x18, 0x48, 0x89, 0x7C, 0x24, 0x20, 0x41, 0x54, 0x41, 0x55, 0x41, 0x56,
            0x41, 0x57, 0x44, 0x8B, 0x54, 0x24, 0x48, 0x33, 0xF6, 0x44, 0x8B, 0x7C, 0x24, 0x60, 0x41, 0x8B,
            0xC2, 0x44, 0x8B, 0x5C, 0x24, 0x58, 0x4D, 0x8B, 0xE1, 0x41, 0x0F, 0xAF, 0xC3, 0x4D, 0x8B, 0xE8,
            0x45, 0x0F, 0xAF, 0xD7, 0x4C, 0x8D, 0x34, 0x82, 0x4A, 0x8D, 0x3C, 0x91, 0x45, 0x85, 0xFF, 0x0F,
            0x84, 0x7B, 0x01, 0x00, 0x00, 0x48, 0x89, 0x5C, 0x24, 0x28, 0x48, 0x89, 0x6C, 0x24, 0x30, 0x49,
            0x8B, 0xE9, 0x48, 0x2B, 0xEF, 0x4D, 0x85, 0xE4, 0x74, 0x07, 0xC5, 0xFA, 0x10, 0x1C, 0x2F, 0xEB,
            0x04, 0xC5, 0xE0, 0x57, 0xDB, 0x8B, 0xC6, 0x33, 0xD2, 0x41, 0x0F, 0xAF, 0xC3, 0x45, 0x33, 0xD2,
            0x4C, 0x8D, 0x0C, 0x85, 0x00, 0x00, 0x00, 0x00, 0x4D, 0x03, 0xCD, 0x45, 0x85, 0xDB, 0x0F, 0x84,
            0x1F, 0x01, 0x00, 0x00, 0x41, 0x83, 0xFB, 0x08, 0x72, 0x6B, 0x45, 0x8B, 0xC3, 0x49, 0x8D, 0x46,
            0x10, 0x41, 0x83, 0xE0, 0xF8, 0x49, 0x8B, 0xC9, 0x49, 0x2B, 0xCE, 0xC5, 0xE8, 0x57, 0xD2, 0xC5,
            0xD8, 0x57, 0xE4, 0x0F, 0x1F, 0x40, 0x00, 0x66, 0x0F, 0x1F, 0x84, 0x00, 0x00, 0x00, 0x00, 0x00,
            0xC5, 0xF8, 0x10, 0x4C, 0x01, 0xF0, 0xC5, 0xF0, 0x59, 0x48, 0xF0, 0xC5, 0xF0, 0x58, 0xD2, 0xC5,
            0xF8, 0x10, 0x08, 0xC5, 0xF0, 0x59, 0x0C, 0x01, 0x83, 0xC2, 0x08, 0x48, 0x8D, 0x40, 0x20, 0x49,
            0x83, 0xC2, 0x08, 0xC5, 0xF0, 0x58, 0xE4, 0x41, 0x3B, 0xD0, 0x72, 0xD4, 0xC5, 0xD8, 0x58, 0xCA,
            0xC5, 0xF0, 0x12, 0xC1, 0xC5, 0xF8, 0x58, 0xD1, 0xC5, 0xE8, 0xC6, 0xC2, 0xF5, 0xC5, 0xEA, 0x58,
            0xD0, 0xC5, 0xE2, 0x58, 0xDA, 0x41, 0x3B, 0xD3, 0x0F, 0x83, 0xA5, 0x00, 0x00, 0x00, 0x41, 0x8B,
            0xC3, 0x2B, 0xC2, 0x83, 0xF8, 0x04, 0x72, 0x75, 0x41, 0x8B, 0xC3, 0x49, 0x8D, 0x4E, 0x04, 0x2B,
            0xC2, 0x4A, 0x8D, 0x0C, 0x91, 0x83, 0xE8, 0x04, 0x4D, 0x8B, 0xC1, 0xC1, 0xE8, 0x02, 0x4D, 0x2B,
            0xC6, 0xFF, 0xC0, 0x8B, 0xD8, 0x8D, 0x14, 0x82, 0x4D, 0x8D, 0x14, 0x82, 0x0F, 0x1F, 0x40, 0x00,
            0xC4, 0xA1, 0x7A, 0x10, 0x44, 0x01, 0xFC, 0xC5, 0xFA, 0x59, 0x49, 0xFC, 0xC4, 0xA1, 0x7A, 0x10,
            0x14, 0x01, 0xC5, 0xEA, 0x59, 0x01, 0x48, 0x8D, 0x49, 0x10, 0xC5, 0xF2, 0x58, 0xDB, 0xC4, 0xA1,
            0x7A, 0x10, 0x4C, 0x01, 0xF4, 0xC5, 0xF2, 0x59, 0x51, 0xF4, 0xC5, 0xE2, 0x58, 0xE0, 0xC4, 0xA1,
            0x7A, 0x10, 0x44, 0x01, 0xF8, 0xC5, 0xFA, 0x59, 0x49, 0xF8, 0xC5, 0xDA, 0x58, 0xDA, 0xC5, 0xE2,
            0x58, 0xD9, 0x48, 0x83, 0xEB, 0x01, 0x75, 0xB8, 0x41, 0x3B, 0xD3, 0x73, 0x26, 0x4D, 0x2B, 0xCE,
            0x4B, 0x8D, 0x0C, 0x96, 0x41, 0x8B, 0xC3, 0x2B, 0xC2, 0x8B, 0xD0, 0xC4, 0xC1, 0x7A, 0x10, 0x04,
            0x09, 0xC5, 0xFA, 0x59, 0x09, 0x48, 0x8D, 0x49, 0x04, 0xC5, 0xE2, 0x58, 0xD9, 0x48, 0x83, 0xEA,
            0x01, 0x75, 0xE8, 0xC5, 0xFA, 0x11, 0x1F, 0x48, 0x83, 0xC7, 0x04, 0xFF, 0xC6, 0x41, 0x3B, 0xF7,
            0x0F, 0x82, 0x9F, 0xFE, 0xFF, 0xFF, 0x48, 0x8B, 0x6C, 0x24, 0x30, 0x48, 0x8B, 0x5C, 0x24, 0x28,
            0x48, 0x8B, 0x74, 0x24, 0x38, 0x48, 0x8B, 0x7C, 0x24, 0x40, 0x41, 0x5F, 0x41, 0x5E, 0x41, 0x5D,
            0x41, 0x5C, 0xC3
        };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public unsafe delegate void _T_FORWARD_KERNEL_AVX(
            float* _Out,       /* [B, O] */
            float* _In,        /* [B, I] */
            float* _Weight,    /* [I, O] */
            float* _Bias,      /* [O] */
            uint b,
            uint B,
            uint I,
            uint O);

        IntPtr _forward_kernel_ptr;
        _T_FORWARD_KERNEL_AVX _forward_kernel_func;

        public MatMulAVX()
            : base(null, matmul_backward) {

            _forward_kernel_ptr = AllocExecuteReadWrite(matmul_forward_kernel);
            _forward_kernel_func = Marshal.GetDelegateForFunctionPointer<_T_FORWARD_KERNEL_AVX>(_forward_kernel_ptr);
        }

        protected override void Dispose(bool disposing) {
            FreeExecuteReadWrite(disposing, ref _forward_kernel_ptr, ref _forward_kernel_func);
            base.Dispose(disposing);
        }

        public override unsafe void forward(
            float* _Out, float* _In, float* _Weight, float* _Bias, uint B, uint I, uint O) {

            if (_maxDegreeOfParallelism == -1 || _maxDegreeOfParallelism > 0) {
                Parallel.For(0, B, (b) => {
                    _forward_kernel_func(
                        _Out,
                        _In,
                        _Weight,
                        _Bias,
                        (uint)b,
                        B,
                        I,
                        O);
                });
            } else {
                for (uint b = 0; b < B; b++) {
                    _forward_kernel_func(
                        _Out,
                        _In,
                        _Weight,
                        _Bias,
                        b,
                        B,
                        I,
                        O);
                }
            }
        }
    }
}