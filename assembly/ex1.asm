format ELF64 executable
segment readable executable
entry main
main:
    mov rax,1
    mov rdi,1
    mov rsi,a
    mov rdx,5
    syscall

    mov rax,60
    mov rdi,0
    syscall
segment readable writable
    a db "test",10