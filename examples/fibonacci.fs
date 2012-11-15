:assembly

32 *org

:code fibonacci
  %a 0x0 # set,
  %b 0x1 # set,
  *begin
    %a %b add,
    %b neg,
    %b %a add,
    %c decr,
  *start
    %c 0x0 # ifn,
  *again
  ret,
;code

0 *org

:code start
  %c 0x0 # set,
  fibonacci call,
  %c 0x1 # set,
  fibonacci call,
  %c 0x8 # set,
  fibonacci call,
  halt,
;code

;assembly

