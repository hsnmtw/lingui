format ELF64 executable
use64

entry main

SYS_READ  = 0
SYS_WRITE = 1
SYS_EXIT  = 60

FD_STDIN  = 0
FD_STDOUT = 1
FD_STDERR = 2

macro error str, len {
    mov rax, SYS_WRITE
    mov rdi, FD_STDERR
    mov rsi, str
    mov rdx, len
    syscall
}

macro write str, len {
    mov rax, SYS_WRITE
    mov rdi, FD_STDOUT
    mov rsi, str
    mov rdx, len
    syscall
}

macro input str, len {
    mov rax, SYS_READ
    mov rdi, FD_STDIN
    mov rsi, str
    mov rdx, len
    syscall
}

macro exit code {
    mov rax, SYS_EXIT
    mov rdi, code
    syscall
}

; -----------------------------------------------------------
; atoi: convert ASCII string at [buffer] into integer in rax
;   - reads until non-digit or null/newline
; -----------------------------------------------------------
macro atoi result, _buffer {
    local .loop, .done
        mov  rax, 0           ; same as (mov rax,0)
        mov  rsi, _buffer
    .loop:
        movzx rcx, byte [rsi]   ; gets the digit at position [rsi] from 
        
        ; if (rcx < '0' || rcx > '9') break;
        cmp  rcx, '0'           ; make sure input is between 0 - 9                  
        jl   .done              ; make sure input is between 0 - 9               
        cmp  rcx, '9'           ; make sure input is between 0 - 9                  
        jg   .done              ; make sure input is between 0 - 9            
        ; rcx -= '0'
        sub  rcx, '0'           ; ASCII digit → numeric value
        ; rax *= 10
        imul rax, rax, 10       ; result = result * 10
        ; rax += rcx
        add  rax, rcx           ; result += digit
        ; rsi += 1
        inc  rsi
        jmp  .loop
    .done:
        ; ret
        mov result, rax
}

; -----------------------------------------------------------
; itoa: convert integer in rax → ASCII in [out_buf]
;   - uses the stack to reverse digits
;   - writes length into [out_len]
; -----------------------------------------------------------
macro itoa src,_out_buf, _out_len {
    local .extract, .push_loop, .pop_loop, .done
        mov  rdi, _out_buf
        mov  rcx, 0             ; digit counter
        mov rax, src
        test rax, rax
        jnz  .extract
        mov  byte [rdi], '0'    ; special-case: input was 0
        mov  qword [_out_len], 1
        ; ret
        jmp .done
    .extract:
        mov  rbx, 10
    .push_loop:
        xor  rdx, rdx
        div  rbx                ; rdx = rax % 10, rax = rax / 10
        add  rdx, '0'
        push rdx                ; push ASCII digit on stack
        inc  rcx
        test rax, rax
        jnz  .push_loop
        ; pop digits off stack into buffer (reverses them)
        mov  rdi, _out_buf
        mov  rbx, rcx           ; save count
    .pop_loop:
        pop  rdx
        mov  byte [rdi], dl
        inc  rdi
        loop .pop_loop
        mov  qword [_out_len], rbx
        ; ret
    .done:
    ; -----------------------------------------------------------
}

main:
    write message, message_len

    input buffer, 32

    ; rax = integer value from input
    atoi rax, buffer

    ; rax = rax * 2  (same as: shl  rax, 1)
    add rax, rax         

    ; writes ASCII result to out_buf
    itoa rax, out_buf, out_len

    write ack, ack_len
    write out_buf, [out_len]
    write newline, 1

    exit 0

; -----------------------------------------------------------
segment readable writeable

message:     db "Enter a number: "
message_len  = $ - message

ack:         db "Doubled: "
ack_len      = $ - ack

newline:     db 10

buffer:      rb 32
out_buf:     rb 32
out_len:     dq 0