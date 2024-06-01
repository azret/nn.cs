// cl /c /Gz /O2 /Ot /fp:fast /Qvec-report:2 MatMul.c

// cl /c /Gz /O2 /Ot /fp:fast /arch:AVX /Qvec-report:2 MatMul.c

// cl /c /Gz /O2 /Ot /fp:fast /arch:AVX2 /Qvec-report:2 MatMul.c

// dumpbin /DISASM:BYTES MatMul.obj > MatMul.asm

void matmul_forward(
    float* _Out,       /* [B, O] */
    float* _In,        /* [B, I] */
    float* _Weight,    /* [I, O] */
    float* _Bias,      /* [O] */
    unsigned int B,
    unsigned int I,
    unsigned int O) {

    for (int b = 0; b < B; b++) {
        float* x = _In + b * I;
        float* y = _Out + b * O;
        for (int o = 0; o < O; o++) {
            float acc = _Bias ? _Bias[o] : 0;
            float* w = _Weight + o * I;
            for (int i = 0; i < I; i++) {
                acc += w[i] * x[i];
            }
            y[o] = (float)acc;
        }
    }
}

void matmul_backward(
    float* _Out,       /* [B, O] */
    float* d_Out,       /* [B, O] */
    float* _In,        /* [B, I] */
    float* d_In,        /* [B, I] */
    float* _Weight,    /* [I, O] */
    float* d_Weight,    /* [I, O] */
    float* _Bias,      /* [O] */
    float* d_Bias,      /* [O] */
    unsigned int B,
    unsigned int I,
    unsigned int O) {

    for (int b = 0; b < B; b++) {
        float* p_In = _In + b * I;
        float* p_d_In = d_In + b * I;
        for (int o = 0; o < O; o++) {
            float δf = d_Out[b * O + o];
            float* p_Weight = _Weight + o * I;
            float* p_d_Weight = d_Weight + o * I;
            for (int i = 0; i < I; i++) {
                p_d_In[i] += p_Weight[i] * δf;
                p_d_Weight[i] += p_In[i] * δf;
            }
            if (d_Bias) {
                d_Bias[o] += δf;
            }
        }
    }
}