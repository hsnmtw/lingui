#!/bin/sh
set -xe

dotnet run --project /mnt/d/code/lingui/lingui.csproj /mnt/d/code/lingui/examples/test.txt > /mnt/d/code/lingui/output/test.asm
fasm /mnt/d/code/lingui/output/test.asm /mnt/d/code/lingui/output/test
/mnt/d/code/lingui/output/test