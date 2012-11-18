:assembly

\ Generates a null-terminated string.
s" Null delimiated string" *ascii
\ Generates a Pascal string (prefixes the string with
\ its length).
s" Pascal string" *pascii
\ Generates a compressed Pascal string (two characters
\ per word).
s" Compressed Pascal strings" *cpascii

;assembly
