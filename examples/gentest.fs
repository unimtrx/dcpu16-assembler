:assembly

\ Tests with basic instructions.
:code gentest-basic
  %a %b set,                     \ SET A, B
  %c [%] %x add,                 \ ADD [C], X
  %y %z [%] sub,                 \ SUB Y, [Z]
  %push 0xffff # mul,            \ MUL PUSH, 0xFFFF
  %peek 0x20 # mli,              \ MLI PEEK, 0x20
  0x800 [#] 0x400 %pick div,     \ DIV [0x800], PICK 0x400
  %sp 0x1 [#] dvi,               \ DVI SP, [0x1]
  0x800 [#] %pc mod,             \ MOD [0x800], PC
  %ex %a 0x50 [%#] mdi,          \ MDI EX, [A + 0x50]
  %b 0x7 [%#] %c and,            \ AND [B + 0x7], C
  %x [%] %y [%] bor,             \ BOR [X], [Y]
  %z [%] 0x5 # xor,              \ XOR [Z], 0x5
  %b [%] 0x500 # shr,            \ SHR [B], 0x500
  %a [%] 0x8 [#] asr,            \ ASR [A], [0x8]
  0x700 [#] %b [%] shl,          \ SHL [0x700], [B]
  %c [%] %x 0x400 [%#] ifb,      \ IFB [C], [X + 0x400]
  %y 0x3 [%#] %z [%] ifc,        \ IFC [Y + 0x3], [Z]
  0x8 [#] 0x5 # ife,             \ IFE [0x8], 0x5
  %b 0xc [%#] 0xf # ifn,         \ IFN [B + 0xC], 0xF
  0xa [#] 0xb00 # ifg,           \ IFG [0xA], 0xB00
  %a 0x9 [%#] 0x20 # ifa,        \ IFA [A + 0x9], 0x20
  0x500 [#] 0x5 [#] ifl,         \ IFL [0x500], [0x5]
  0x600 [#] %a 0x700 [%#] ifu,   \ IFU [0x600], [A + 0x700]
  %b 0x800 [%#] 0x9 [#] adx,     \ ADX [B + 0x800], [0x9]
  %c 0xa00 [%#] %x 0xb [%#] sbx, \ SBX [C + 0xA00], [X + 0xB]
;code

\ Tests with special instructions.
:code gentest-special
  %a jsr,                        \ JSR A
  %b [%] int,                    \ INT [B]
  0xffff # iag,                  \ IAG 0xFFFF
  0x1e # ias,                    \ IAS 0x1E
  0xfffe # rfi,                  \ RFI 0xFFFE
  0x1f # iaq,                    \ IAQ 0x1F
  0x0 [#] hwn,                   \ HWN [0x0]
  0x20 [#] hwq,                  \ HWQ [0x20]
  %a 0x10 [%#] hwi,              \ HWI [A + 0x10]
;code

;assembly

