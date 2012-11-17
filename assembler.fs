\ Assembler for assembly in Forth.
variable block-current
variable block-buffer
variable pc
variable depth
\ Allows for a maximum of (n - 2) breaks in k levels,
\ where k * n = 64. This implementation considers
\ n = 8 and k = 8.
create resolve-array 0x40 cells allot

: @1-! ( addr -- ) dup @ 1- swap ! ;
: @1+! ( addr -- ) dup @ 1+ swap ! ;

: resolve ( n -- addr ) 0x8 ( n = 8 ) * cells resolve-array + ;
: #resolve! ( n -- ) resolve ! ;
: #resolve@ ( n -- n ) resolve @ ;
: resolvepc! ( n -- ) resolve cell + pc @ 1- swap ! ;
: resolvepc@ ( n -- n ) resolve cell + @ ;
: resolve! ( val i n -- ) resolve swap 0x2 + cells + ! ;
: resolve@ ( i n -- ) resolve swap 0x2 + cells + @ ;

: update-block pc @ 2* 0x400 / dup block-current @ >
  if dup dup 1+ block-current @ 1+ ?do i block 0x400 0x0 fill loop
    block pc @ 0x400 mod 2* + block-buffer ! block-current !
  else drop then ;

: update-buffer update-block block-current @ block pc @ 2* 0x400 mod + block-buffer ! ;

: word, ( n -- ) update-block pc @1+! block-buffer @ w!
  block-buffer @ 0x2 + block-buffer ! ;

: instruction ( -- addr ) block-buffer @ 0x0 word, ;

: depth@ ( -- n ) depth @ ;
: depth+1! ( -- ) depth @ 0x8 ( k = 8 ) =
  if 1 throw then
  depth @1+! 0x0 depth@ resolve ! ;
: depth-1! ( -- ) depth@ #resolve@ 0x0
  ?do pc @ 1- i depth@ resolve@ w! loop depth @1-! ;

: :assembly ( -- ) 0x0 pc ! -0x1 block-current ! update-block ;
\ 0x0 block block-buffer ! ;
: ;assembly ( -- ) update flush ;

: :code ( -- addr addr )
  pc @ constant instruction -0x1 depth ! ;
: ;code ( addr -- )
  pc @1-! block-buffer @ 0x2 - block-buffer ! ;

\ Could be done with CREATE-DOES>.
: 'basic-opcode ( addr operand operand instruction -- addr )
  swap 0xa lshift or swap 0x5 lshift or swap w! instruction ;
: basic-opcode ( n -- ) >r : ['] lit compile,
  r> , ['] 'basic-opcode compile, postpone ; ;

: 'special-opcode ( addr operand operand instruction -- addr )
  swap 0xa lshift or swap w! instruction ;
: special-opcode ( n -- ) 0x5 lshift >r : ['] lit compile,
  r> , ['] 'special-opcode compile, postpone ; ;

0x00 constant %a
0x01 constant %b
0x02 constant %c
0x03 constant %x
0x04 constant %y
0x05 constant %z
0x06 constant %i
0x07 constant %j
0x1b constant %sp
0x1c constant %pc
0x1d constant %ex
0x18 constant %push
0x18 constant %pop
0x19 constant %peek
: %pick word, 0x1a ;

0x01 basic-opcode set,
0x02 basic-opcode add,
0x03 basic-opcode sub,
0x04 basic-opcode mul,
0x05 basic-opcode mli,
0x06 basic-opcode div,
0x07 basic-opcode dvi,
0x08 basic-opcode mod,
0x09 basic-opcode mdi,
0x0a basic-opcode and,
0x0b basic-opcode bor,
0x0c basic-opcode xor,
0x0d basic-opcode shr,
0x0e basic-opcode asr,
0x0f basic-opcode shl,
0x10 basic-opcode ifb,
0x11 basic-opcode ifc,
0x12 basic-opcode ife,
0x13 basic-opcode ifn,
0x14 basic-opcode ifg,
0x15 basic-opcode ifa,
0x16 basic-opcode ifl,
0x17 basic-opcode ifu,
0x1a basic-opcode adx,
0x1b basic-opcode sbx,
0x1e basic-opcode sti,
0x1f basic-opcode std,

0x01 special-opcode jsr,
0x08 special-opcode int,
0x09 special-opcode iag,
0x0a special-opcode ias,
0x0b special-opcode rfi,
0x0c special-opcode iaq,
0x10 special-opcode hwn,
0x11 special-opcode hwq,
0x12 special-opcode hwi,

\ Modifiers.
: # ( n -- operand ) dup dup 0x0 0x1f within swap 0xffff = or
    if 0x21 + else word, 0x1f then ;
: [%] ( reg -- operand ) 0x8 + ;
: [#] ( n -- operand ) word, 0x1e ;
: [%#] ( reg n -- operand ) word, 0x10 + ;

\ Macros.
: decr, 0x1 # sub, ;
: incr, 0x1 # add, ;
: zero, 0x0 # set, ;
: push, %push swap set, ;
: goto, %pc swap set, ;
: halt, %pc decr, ;
: call, # jsr, ;
: ret, %pc %pop set, ;
: pop, %pop set, ;
: neg, 0xffff # mul, ;

\ Flow control constructs.
\ Breaks out n levels.
\ : *nbreak ( n -- ) depth@ swap - >r 0x1f goto, r@ #resolve@ dup
\    1+ r@ #resolve! r@ resolve! rdrop instruction ;
: *nbreak ( n -- )depth@ swap - >r 0x1f goto, r@ #resolve@ dup
    1+ r@ #resolve! r@ resolve! rdrop instruction ;
\ Breaks out from the current level.
: *break ( -- ) 0x0 *nbreak ;
\ Breaks out to the first outter level.
: *breakout ( -- ) 0x1 *nbreak ;

\ *mark --> $1:
\  ...      ...
\ *goto --> set pc, $1
: *mark ( -- ) depth+1! depth@ resolvepc! ;
: *goto ( -- ) depth@ resolvepc@ # goto, depth-1! ;

\ *repeat --> $1:
\  ...        ...
\ *again      set pc, $1
: *repeat ( -- ) *mark ;
: *again  ( -- ) *goto ;

\ *begin --> goto $2
\ ...        $1:
\ ...        ...
\ *start     $2:
\ ...        ...
\ *again     set pc, $1
: *begin ( -- addr ) 0x1f goto, instruction *mark ;
: *start ( addr1 addr2 -- addr2 ) swap pc @ 1- swap w! ;

\ *jump --> set pc, $1
\ ...       ...
\ *here --> $1:
: *jump *mark *break ;
: *here depth-1! ;

\ Other constructs.
: *org pc ! update-buffer ;
: *label pc @ constant ;
: *dat word, ;
: *ascii dup word, 0 ?do dup c@ word, 1+ loop ;

