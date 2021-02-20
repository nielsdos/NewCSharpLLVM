#include <stdio.h>

int main(int argc, char* argv[]) {
    extern float test(float, long);
    printf("%g\n", test(3.14f, 3));
    return 0;
}