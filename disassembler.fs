variable block-current
variable block-buffer
variable pc
variable org

\ Holds the 'next words'.
create next-words 0x3 cells allot

: @1-! ( addr -- ) dup @ 1- swap ! ;
: @1+! ( addr -- ) dup @ 1+ swap ! ;

\ Stores the TOS in the appropriate index of next-words.
: next-word! ( n -- )
  next-words @1+! next-words dup @ cells + ! ;
\ Fetches the next 'next word'.
: next-word@ ( -- n )
  next-words dup @ cells + @ next-words @1-! ;

\ Fetches a new block if needed.
: update-block ( -- ) pc @ 2* 0x400 / block-current @ > if
    block-current @1+! block-current @
    block block-buffer !
  else
    block-buffer dup @ 0x2 + swap !
  then ;

\ Gets the next word in the current block.
: word@ update-block block-buffer @ w@ pc @1+! ;

\ Holds a string for each register.
\ Holds one character for the size and a maximum of four
\ characters for the string.
create register-array 0x92 ( 0x1d * 0x5 ) allot
register-array 0x92 0x0 fill
: register ( c-addr n reg -- )
  0x5 * register-array + >r dup r@ c! r@ 1+ swap cmove rdrop ;
: lookup-register ( reg -- c-addr n )
  0x5 * register-array + >r r@ 1+ r@ c@ rdrop ;

s" a" 0x0 register
s" b" 0x1 register
s" c" 0x2 register
s" x" 0x3 register
s" y" 0x4 register
s" z" 0x5 register
s" i" 0x6 register
s" j" 0x7 register
s" push" 0x18 register
s" peek" 0x19 register
s" pick" 0x1a register
s" sp" 0x1b register
s" pc" 0x1c register
s" ex" 0x1d register

\ Holds a string for each basic instruction.
create basic-array 0x5d allot
basic-array 0x5d 0x0 fill
\ Stores a new string for a basic instruction.
: basic-instruction ( c-addr n ins -- )
  0x3 * basic-array + swap cmove ;
\ Check if a basic instruction with opcode == TOS exists.
: basic-contains ( n -- flag ) 0x3 * basic-array + c@ 0<> ;
\ Gets the string associated with the opcode.
: lookup-basic ( ins -- c-addr n ) 0x3 * basic-array + 0x3 ;

s" set" 0x01 basic-instruction
s" add" 0x02 basic-instruction
s" sub" 0x03 basic-instruction
s" mul" 0x04 basic-instruction
s" mli" 0x05 basic-instruction
s" div" 0x06 basic-instruction
s" dvi" 0x07 basic-instruction
s" mod" 0x08 basic-instruction
s" mdi" 0x09 basic-instruction
s" and" 0x0a basic-instruction
s" bor" 0x0b basic-instruction
s" xor" 0x0c basic-instruction
s" shr" 0x0d basic-instruction
s" asr" 0x0e basic-instruction
s" shl" 0x0f basic-instruction
s" ifb" 0x10 basic-instruction
s" ifc" 0x11 basic-instruction
s" ife" 0x12 basic-instruction
s" ifn" 0x13 basic-instruction
s" ifg" 0x14 basic-instruction
s" ifa" 0x15 basic-instruction
s" ifl" 0x16 basic-instruction
s" ifu" 0x17 basic-instruction
s" ifg" 0x18 basic-instruction
s" adx" 0x1a basic-instruction
s" sbx" 0x1b basic-instruction
s" sti" 0x1e basic-instruction
s" std" 0x1f basic-instruction

\ Holds a string for each special instruction.
create special-array 0x36 allot
special-array 0x36 0x0 fill
: special-instruction ( c-addr n ins -- )
  0x3 * special-array + swap cmove ;
: special-contains ( n -- flag ) dup 0x12 > if drop false else
    0x3 * special-array + c@ 0<> then ;
: lookup-special ( ins -- c-addr n ) 0x3 * special-array + 0x3 ;

s" jsr" 0x01 special-instruction
s" int" 0x08 special-instruction
s" iag" 0x09 special-instruction
s" ias" 0x0a special-instruction
s" rfi" 0x0b special-instruction
s" iaq" 0x0c special-instruction
s" hwn" 0x10 special-instruction
s" hwq" 0x11 special-instruction
s" hwi" 0x12 special-instruction

: disassemble-operand ( operand -- )
  \ register
  dup 0x0 0x8 within if ." %" lookup-register type exit then
  \ %pc, %sp, %ex and %peek
  dup 2dup 0x18 = swap 0x19 = or swap 0x1b 0x1e within or
  if ." %" lookup-register type exit then
  \ [register]
  dup 0x8 0x10 within
  if 0x8 - ." %" lookup-register type ."  [%]" exit then
  \ [register + literal]
  dup 0x10 0x18 within if
    0x10 - ." %" lookup-register type space
    next-word@ . ." [%#]" exit
  then
  \ short literal
  dup 0x20 0x40 within if
    0x21 - . ." #" exit
  then
  \ %pick
  dup 0x1a = if
    next-word@ . ." %pick" drop exit
  then
  \ [literal]
  dup 0x1e = if
    next-word@ . ." [#]" drop exit
  then
  \ long literal
  0x1f = if
    next-word@ . ." #" exit
  then
  \ Invalid operand.
  0x1 throw
;

: requires-next-word? ( operand -- flag )
  >r r@ 0x10 0x18 within
  r@ 0x1a = or
  r@ 0x1e = or
  r> 0x1f = or ;

: disassemble-instruction ( opcode -- )
  \ Replace sequential zeroes with *ORG.
  dup 0x0 = if pc @ org ! drop exit then
  org @ 0x0 <> if org @ . ."  *org" cr 0x0 org ! then
  0x0 next-words !
  \ Check for a special instruction.
  >r r@ 0x1f and 0= if
    r@ 0x3e0 and 0x5 rshift dup special-contains if
      \ Store the 'next word'.
      r@ 0xfc00 and 0xa rshift dup
      requires-next-word? if word@ next-word! then
      \ Print the operand.
      disassemble-operand space
      \ Print the instruction.
      lookup-special type ." ," rdrop cr exit
    else drop then
  else
    \ Check for a basic instruction.
    r@ 0x1f and dup basic-contains if
      \ Store the next words.
      r@ 0xfc00 and 0xa rshift dup
      requires-next-word? if word@ next-word! then
      r@ 0x03e0 and 0x5 rshift dup
      requires-next-word? if word@ next-word! then
      \ Print the destination operand.
      disassemble-operand space
      \ Print the source operand.
      dup 0x18 = if drop ." %pop"
      else disassemble-operand then space
      lookup-basic type ." ," rdrop cr exit
    then
  then
  r> . ." *dat" cr
;

\ Disassembles the first block (block 0).
\ TODO: Disassemble all blocks in a given range.
: disassemble ( -- ) -0x1 block-current ! 0x0 pc !
  begin pc @ 0x200 < while
    word@ disassemble-instruction repeat ;

hex disassemble

