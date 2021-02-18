#include <stdio.h>

int main(int argc, char* argv[]) {
    extern int test(int, int);
    printf("%d\n", test(5, 8));
    return 0;
}