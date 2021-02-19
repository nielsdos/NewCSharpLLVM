#include <stdio.h>

static int fib(int n)
{
    if(n < 2) return 1;
    return fib(n - 1) + fib(n - 2);
}

int main(int argc, char* argv[]) {
    extern int test(int);
    for(int i = 0;i<10;++i)
        printf("%d %d\n", test(i), fib(i));
    return 0;
}