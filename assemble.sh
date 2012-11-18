#! /usr/bin/env bash


if [ $# -ne 1 ]; then
  echo "ERROR: $0 expects a Forth program as the parameter."
  exit 65
fi

if [ ! -f "$1" ]; then
  echo "ERROR: File does not exist."
  exit 127
fi

OUT="`basename "$1" ".fs"`.bin"

[ -f $OUT ] && rm $OUT

gforth -e "s\" $OUT\" open-blocks" assembler.fs "$1" -e 'bye'

