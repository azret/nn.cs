// cl /c /Gz /O2 /Ot /fp:fast /Qvec-report:2 MatMul.c

// cl /c /Gz /O2 /Ot /fp:fast /arch:AVX /Qvec-report:2 MatMul.c

// cl /c /Gz /O2 /Ot /fp:fast /arch:AVX2 /Qvec-report:2 MatMul.c

// dumpbin /DISASM:BYTES MatMul.obj > MatMul.asm

typedef struct {
    float* _Out;       /* [B, O] */
    float* d_Out;       /* [B, O] */
    float* _In;        /* [B, I] */
    float* d_In;        /* [B, I] */
    float* _Weight;    /* [I, O] */
    float* d_Weight;    /* [I, O] */
    float* _Bias;      /* [O] */
    float* d_Bias;      /* [O] */
    unsigned int B;
    unsigned int I;
    unsigned int O;
} MatMul;

void _ASM_FORWARD_KERNEL_AVX2(MatMul* args, unsigned int bo) {
    float* _Out = args->_Out;
    float* _In = args->_In;
    float* _Weight = args->_Weight;
    float* _Bias = args->_Bias;

    unsigned int B = args->B;
    unsigned int I = args->I;
    unsigned int O = args->O;

    unsigned int b = bo / O;
    unsigned int o = bo % O;

    if (b < B && o < O) {
        float* x = _In + b * I;
        float* y = _Out + b * O;
        float acc = _Bias ? _Bias[o] : 0;
        float* w = _Weight + o * I;
        for (int i = 0; i < I; i++) {
            acc += w[i] * x[i];
        }
        y[o] = (float)acc;
    }
}

void _ASM_FORWARD_AVX2(MatMul* args) {
    float* _Out = args->_Out;
    float* _In = args->_In;
    float* _Weight = args->_Weight;
    float* _Bias = args->_Bias;

    unsigned int B = args->B;
    unsigned int I = args->I;
    unsigned int O = args->O;

    for (unsigned int bo = 0; bo < B * O; bo++) {

        unsigned int b = bo / O;
        unsigned int o = bo % O;

        if (b < B && o < O) {
            float* x = _In + b * I;
            float* y = _Out + b * O;
            float acc = _Bias ? _Bias[o] : 0;
            float* w = _Weight + o * I;
            for (int i = 0; i < I; i++) {
                acc += w[i] * x[i];
            }
            y[o] = (float)acc;
        }
    }
}

void _ASM_MATMUL_BACKWARD_KERNAL_AVX2_I(MatMul* args, unsigned int b) {
    float* d_Out = args->d_Out;
    float* _In = args->_In;
    float* d_In = args->d_In;
    float* _Weight = args->_Weight;
    float* d_Weight = args->d_Weight;
    float* d_Bias = args->d_Bias;

    unsigned int B = args->B;
    unsigned int I = args->I;
    unsigned int O = args->O;

    for (unsigned int o = 0; o < O; o++) {
        float* d_In_b = d_In + b * I;
        float* _Weight_o = _Weight + o * I;
        float d = d_Out[b * O + o];
        for (unsigned int i = 0; i < I; i++) {
            d_In_b[i] += _Weight_o[i] * d;
        }
    }
}

void _ASM_MATMUL_BACKWARD_KERNAL_AVX2_II(MatMul* args, unsigned int o) {
    float* d_Out = args->d_Out;
    float* _In = args->_In;
    float* d_In = args->d_In;
    float* _Weight = args->_Weight;
    float* d_Weight = args->d_Weight;
    float* d_Bias = args->d_Bias;

    unsigned int B = args->B;
    unsigned int I = args->I;
    unsigned int O = args->O;

    for (unsigned int b = 0; b < B; b++) {
        float* _In_b = _In + b * I;
        float* δ_Weight_o = d_Weight + o * I;
        float δ = d_Out[b * O + o];
        for (unsigned int i = 0; i < I; i++) {
            δ_Weight_o[i] += _In_b[i] * δ;
        }
        if (d_Bias) {
            d_Bias[o] += δ;
        }
    }
}

void _ASM_BACKWARD_AVX2(MatMul* args) {
    float* d_Out = args->d_Out;
    float* _In = args->_In;
    float* d_In = args->d_In;
    float* _Weight = args->_Weight;
    float* d_Weight = args->d_Weight;
    float* d_Bias = args->d_Bias;

    unsigned int B = args->B;
    unsigned int I = args->I;
    unsigned int O = args->O;

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