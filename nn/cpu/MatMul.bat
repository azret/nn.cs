cls
del MatMul.obj
cl.exe /c /Gz /O2 /Ot /fp:fast /Qvec-report:2 /arch:AVX MatMul.c
dumpbin /DISASM:BYTES MatMul.obj > MatMul.asm
..\..\tools\coff\bin\x64\Debug\coff.exe MatMul.obj  > MatMul.txt