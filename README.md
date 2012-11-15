DCPU-16 Forth Assembler
=======================

Introduction
------------

This assembler implements the DCPU-16 version 1.7 instruction set and some
common assembler directives embedded in the Forth language. It was made to be
used along with an upcoming standalone Forth operating system for the 0x10c
game. See http://0x10c.com and http://c2.com/cgi/wiki?ForthLanguage for more
information. Also, take a look at the DCPU-16 version 1.7 specification at
http://pastebin.com/raw.php?i=Q4JvQvnM.

Prerequisites
-------------

A Forth interpreter is required to run the assembler. This README assumes that
you are using Gforth 0.7.0. You can install it on Debian-based systems by
executing:

````bash
apt-get install gforth
````

Usage
-----

Open up the Gforth interpreter and follow along.
In case you can't figure it out, it's done by typing:
````bash
gforth
````
in a terminal.

### Specifying an output file
First we need to specify the output file. The assembler, however, works with
blocks, not files. This behavior is also common on older Forth systems.
In Gforth, we can map Forth blocks to a file.
Here's how this is done:
````forth
s" <blocks-file>" open-blocks
````
Replace `<blocks-file>` with the path of the desired output file.
Note that the file data is in little endian.
All blocks in the block file remain unmodified by the assembler unless data is
written to it. As such, it is recommended that the block file be removed prior
to being used again.

### Loading assembler definitions
We also need to load the assembler definitions. This is done as follows:
````forth
s" assembler.fs" included
````

### Assembling the program
Now we need to load and interpret the file containing the program.
It's just like loading the assembler, here's how:
````forth
s" <program-file>" included
````
Replace `<program-file>` with the path of the file containing the program.
This should assemble the program and write the resulting data to the provided
output file.

### Exiting the interpreter
To exit the interpreter you can use Ctrl-D to send an EOF, or you can type:
````forth
bye
````

### An easier way
This can all be done in a much simpler way:
````bash
gforth -e 's" <blocks-file>" open-blocks' assembler.fs <program-file> -e 'bye'
````

Assisted assembly
-----------------

Assisted assembly in Forth (referred to as FA from now on) is different from
standard assembly. Below are some examples comparing both of these.
First, some simple mappings for operands from standard DCPU-16 assembly to
FA.

| Standard | FA |
| -------- | -----------  |
| `1, 0x10`  | `1 #, 0x10 #` |
| `a, i, x`  | `%a,  %i, %x` |
| `pc, ex, sp` | `%pc, %ex, %sp` |
| `pick 5`   | `5 %pick` |
| `[1], [0x10]` | `1 [#], 0x10 [#]` |
| `[a + 0x1]` | `%a 0x1 [%#]` |
| `[a - 0x2]` | `%a -0x2 [%#]` |
| `[a], [b]`  | `%a [%], %b [%]` |
| `set` | `set,` |
| `jsr` | `jsr,` |
| `"String"` | `s" String"` |

As some of you may have noticed, Forth uses postfix notation, as does the
assembler. What follows is an example of standard DCPU-16 constructs and
instructions converted to Forth assembly.

| Standard | FA |
| -------- | -----------  |
| `set a, b` | `%a %b set,` |
| `mul c, 0x10` | `%c 0x10 # mul,` |
| `set [a], 0x0` | `%a [%] 0x0 # set,` |
| `sti [i + 1], j` | `%i 0x1 [%#] %j sti,` |
| `hwn i` | `%i hwn` |
| `dat 0x5` | `0x5 *dat` |
| `dat 0x6, "String"` | `s" String" *ascii` |

The assembler supports labels, but they shouldn't be used inside assembly code
definitions. Instead, use the provided control flow constructs. Here is an
example of a control flow construct:

| Standard | FA |
| -------- | ----------- |
| `:begin` | `*begin` |
| `sub i, 0x1` | `%i 0x1 # sub,` |
| `ifn i, 0x0` | `%i 0x0 # ifn,` |
| `set pc, begin` | `*repeat` |

Here's an example comparing a minimal function definition in standard assembly
and one in FA.

| Standard | FA |
| -------- | ----------- |
| `:gimme_one` | `:code gimme-one` |
| `  set a, 0x1` | `  %a 0x1 # set,` |
| `  set pc, pop` | `  %pc %pop set,` |
| | `;code` | 

More examples and explanations can be found in the `examples/` and `docs/`
directories.


