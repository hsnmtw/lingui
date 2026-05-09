format ELF64 executable 3
use64
segment readable executable
SYS_READ  = 0
SYS_WRITE = 1
SYS_EXIT  = 60

FD_STDIN  = 0
FD_STDOUT = 1
FD_STDERR = 2
macro print str,len {
    mov rax,SYS_WRITE
    mov rdi,FD_STDOUT
    mov rsi,str
    mov rdx,len
    syscall
}
macro input str,len {
    mov rax,SYS_READ
    mov rdi,FD_STDIN
    mov rsi,str
    mov rdx,len
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
entry main
main:
   print db2,db2_len
   call print_newline
   print db3,db3_len
   call print_newline
   print db4,db4_len
   print db1,db1_len
   call print_newline
   print db5,db5_len
   print db0,db0_len
   call print_newline
   print db_1,db_1_len
   input db1,db1_len
   print db6,db6_len
   print db1,db1_len
   call print_newline
   print db7,db7_len
   print db0,db0_len
   call print_newline
   exit 0
   ;;;;;;
print_newline:
   print newline,1
   ret
segment readable writable
newline   db 10
db0    : db "71"
db0_len=$-db0
db1    : db "85"
db1_len=$-db1
db2    : db "10"
db2_len=$-db2
db3    : db "Hello World"
db3_len=$-db3
db4    : db "a="
db4_len=$-db4
db5    : db "n="
db5_len=$-db5
db_1:db "Enter a value for a="
db_1_len=$-db_1
db6    : db "a="
db6_len=$-db6
db7    : db "n="
db7_len=$-db7

