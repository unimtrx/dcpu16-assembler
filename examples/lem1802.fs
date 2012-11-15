:assembly

32 *org

*label hello-string
s" You will be assimilated." *ascii

:code init-lem1802
  \ MEM_MAP_SCREEN
  %a 0x0 # set,
  %b 0x8000 # set,
  %i hwi,
  \ SET_BORDER_COLOR
  %a 0x3 # set,
  %b 0x8 # set,
  %i hwi,
  ret,
;code

:code init-hardware
  %i hwn,
  *repeat
    %i decr,
    %i hwq,
    %b 0x7349 # ife,
    %a 0xf615 # ife,
    init-lem1802 call,
    %i 0x0 # ifn,
  *again
  ret,
;code

:code print-hello
  %a hello-string [#] set,
  %i 0x0 # set,
  %j hello-string 1+ # set,
  *repeat
    %i %a ife,
    *break
    %b %j [%] set,
    %b 0xa000 # bor,
    %i 0x8000 [%#] %b sti,
  *again
  ret,
;code

0 *org

:code start
  init-hardware call,
  print-hello call,
  halt,
;code

;assembly

