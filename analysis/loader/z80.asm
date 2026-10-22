; BINSPY: interpretación lineal Z80, NO identificación de código.
; Datos mezclados, prefijos ignorados y opcodes no documentados pueden alterar la lectura.
; Dirección | bytes | instrucción | offset absoluto del archivo
0000  24            INC H                        ; archivo 0x00000000
0001  00            NOP                          ; archivo 0x00000001
0002  0A            LD A,(BC)                    ; archivo 0x00000002
0003  00            NOP                          ; archivo 0x00000003
0004  AD            XOR L                        ; archivo 0x00000004
0005  200F          JR NZ,&0016                  ; archivo 0x00000005
0007  018220        LD BC,&2082                  ; archivo 0x00000007
000A  0E01          LD C,&01                     ; archivo 0x0000000A
000C  A2            AND D                        ; archivo 0x0000000C
000D  200E          JR NZ,&001D                  ; archivo 0x0000000D
000F  2C            INC L                        ; archivo 0x0000000F
0010  0E01          LD C,&01                     ; archivo 0x00000010
0012  A2            AND D                        ; archivo 0x00000012
0013  200F          JR NZ,&0024                  ; archivo 0x00000013
0015  2C            INC L                        ; archivo 0x00000015
0016  19            ADD HL,DE                    ; archivo 0x00000016
0017  1A            LD A,(DE)                    ; archivo 0x00000017
0018  01B722        LD BC,&22B7                  ; archivo 0x00000018
001B  64            LD H,H                       ; archivo 0x0000001B
001C  2201AA        LD (&AA01),HL                ; archivo 0x0000001C
001F  201C          JR NZ,&003D                  ; archivo 0x0000001F
0021  FF            RST &38                      ; archivo 0x00000021
0022  05            DEC B                        ; archivo 0x00000022
0023  00            NOP                          ; archivo 0x00000023
0024  2A0014        LD HL,(&1400)                ; archivo 0x00000024
0027  00            NOP                          ; archivo 0x00000027
0028  A9            XOR C                        ; archivo 0x00000028
0029  2019          JR NZ,&0044                  ; archivo 0x00000029
002B  0A            LD A,(BC)                    ; archivo 0x0000002B
002C  2C            INC L                        ; archivo 0x0000002C
002D  19            ADD HL,DE                    ; archivo 0x0000002D
002E  0A            LD A,(BC)                    ; archivo 0x0000002E
002F  01A322        LD BC,&22A3                  ; archivo 0x0000002F
0032  47            LD B,A                       ; archivo 0x00000032
0033  72            LD (HL),D                    ; archivo 0x00000033
0034  61            LD H,C                       ; archivo 0x00000034
0035  6E            LD L,(HL)                    ; archivo 0x00000035
0036  64            LD H,H                       ; archivo 0x00000036
0037  65            LD H,L                       ; archivo 0x00000037
0038  2076          JR NZ,&00B0                  ; archivo 0x00000038
003A  69            LD L,C                       ; archivo 0x0000003A
003B  74            LD (HL),H                    ; archivo 0x0000003B
003C  65            LD H,L                       ; archivo 0x0000003C
003D  73            LD (HL),E                    ; archivo 0x0000003D
003E  73            LD (HL),E                    ; archivo 0x0000003E
003F  65            LD H,L                       ; archivo 0x0000003F
0040  2028          JR NZ,&006A                  ; archivo 0x00000040
0042  6F            LD L,A                       ; archivo 0x00000042
0043  2F            CPL                          ; archivo 0x00000043
0044  6E            LD L,(HL)                    ; archivo 0x00000044
0045  29            ADD HL,HL                    ; archivo 0x00000045
0046  223B03        LD (&033B),HL                ; archivo 0x00000046
0049  00            NOP                          ; archivo 0x00000049
004A  00            NOP                          ; archivo 0x0000004A
004B  76            HALT                         ; archivo 0x0000004B
004C  E9            JP (HL)                      ; archivo 0x0000004C
004D  00            NOP                          ; archivo 0x0000004D
004E  2D            DEC L                        ; archivo 0x0000004E
004F  00            NOP                          ; archivo 0x0000004F
0050  1E00          LD E,&00                     ; archivo 0x00000050
0052  A9            XOR C                        ; archivo 0x00000052
0053  2016          JR NZ,&006B                  ; archivo 0x00000053
0055  2C            INC L                        ; archivo 0x00000055
0056  19            ADD HL,DE                    ; archivo 0x00000056
0057  0C            INC C                        ; archivo 0x00000057
0058  01A322        LD BC,&22A3                  ; archivo 0x00000058
005B  45            LD B,L                       ; archivo 0x0000005B
005C  71            LD (HL),C                    ; archivo 0x0000005C
005D  75            LD (HL),L                    ; archivo 0x0000005D
005E  69            LD L,C                       ; archivo 0x0000005E
005F  70            LD (HL),B                    ; archivo 0x0000005F
0060  65            LD H,L                       ; archivo 0x00000060
0061  6D            LD L,L                       ; archivo 0x00000061
0062  65            LD H,L                       ; archivo 0x00000062
0063  6E            LD L,(HL)                    ; archivo 0x00000063
0064  74            LD (HL),H                    ; archivo 0x00000064
0065  2063          JR NZ,&00CA                  ; archivo 0x00000065
0067  6F            LD L,A                       ; archivo 0x00000067
0068  6D            LD L,L                       ; archivo 0x00000068
0069  70            LD (HL),B                    ; archivo 0x00000069
006A  6C            LD L,H                       ; archivo 0x0000006A
006B  65            LD H,L                       ; archivo 0x0000006B
006C  74            LD (HL),H                    ; archivo 0x0000006C
006D  2028          JR NZ,&0097                  ; archivo 0x0000006D
006F  6F            LD L,A                       ; archivo 0x0000006F
0070  2F            CPL                          ; archivo 0x00000070
0071  6E            LD L,(HL)                    ; archivo 0x00000071
0072  29            ADD HL,HL                    ; archivo 0x00000072
0073  223B03        LD (&033B),HL                ; archivo 0x00000073
0076  00            NOP                          ; archivo 0x00000076
0077  00            NOP                          ; archivo 0x00000077
0078  65            LD H,L                       ; archivo 0x00000078
0079  F1            POP AF                       ; archivo 0x00000079
007A  00            NOP                          ; archivo 0x0000007A
007B  2A0028        LD HL,(&2800)                ; archivo 0x0000007B
007E  00            NOP                          ; archivo 0x0000007E
007F  A9            XOR C                        ; archivo 0x0000007F
0080  2019          JR NZ,&009B                  ; archivo 0x00000080
0082  0A            LD A,(BC)                    ; archivo 0x00000082
0083  2C            INC L                        ; archivo 0x00000083
0084  19            ADD HL,DE                    ; archivo 0x00000084
0085  0E01          LD C,&01                     ; archivo 0x00000085
0087  A3            AND E                        ; archivo 0x00000087
0088  225469        LD (&6954),HL                ; archivo 0x00000088
008B  72            LD (HL),D                    ; archivo 0x0000008B
008C  73            LD (HL),E                    ; archivo 0x0000008C
008D  2069          JR NZ,&00F8                  ; archivo 0x0000008D
008F  6E            LD L,(HL)                    ; archivo 0x0000008F
0090  66            LD H,(HL)                    ; archivo 0x00000090
0091  69            LD L,C                       ; archivo 0x00000091
0092  6E            LD L,(HL)                    ; archivo 0x00000092
0093  69            LD L,C                       ; archivo 0x00000093
0094  73            LD (HL),E                    ; archivo 0x00000094
0095  2020          JR NZ,&00B7                  ; archivo 0x00000095
0097  286F          JR Z,&0108                   ; archivo 0x00000097
0099  2F            CPL                          ; archivo 0x00000099
009A  6E            LD L,(HL)                    ; archivo 0x0000009A
009B  29            ADD HL,HL                    ; archivo 0x0000009B
009C  223B03        LD (&033B),HL                ; archivo 0x0000009C
009F  00            NOP                          ; archivo 0x0000009F
00A0  00            NOP                          ; archivo 0x000000A0
00A1  74            LD (HL),H                    ; archivo 0x000000A1
00A2  E9            JP (HL)                      ; archivo 0x000000A2
00A3  2000          JR NZ,&00A5                  ; archivo 0x000000A3
00A5  27            DAA                          ; archivo 0x000000A5
00A6  00            NOP                          ; archivo 0x000000A6
00A7  3200A9        LD (&A900),A                 ; archivo 0x000000A7
00AA  2019          JR NZ,&00C5                  ; archivo 0x000000AA
00AC  0A            LD A,(BC)                    ; archivo 0x000000AC
00AD  2C            INC L                        ; archivo 0x000000AD
00AE  19            ADD HL,DE                    ; archivo 0x000000AE
00AF  1001          DJNZ &00B2                   ; archivo 0x000000AF
00B1  A3            AND E                        ; archivo 0x000000B1
00B2  22496E        LD (&6E49),HL                ; archivo 0x000000B2
00B5  76            HALT                         ; archivo 0x000000B5
00B6  75            LD (HL),L                    ; archivo 0x000000B6
00B7  6C            LD L,H                       ; archivo 0x000000B7
00B8  6E            LD L,(HL)                    ; archivo 0x000000B8
00B9  65            LD H,L                       ; archivo 0x000000B9
00BA  72            LD (HL),D                    ; archivo 0x000000BA
00BB  61            LD H,C                       ; archivo 0x000000BB
00BC  62            LD H,D                       ; archivo 0x000000BC
00BD  6C            LD L,H                       ; archivo 0x000000BD
00BE  65            LD H,L                       ; archivo 0x000000BE
00BF  2028          JR NZ,&00E9                  ; archivo 0x000000BF
00C1  6F            LD L,A                       ; archivo 0x000000C1
00C2  2F            CPL                          ; archivo 0x000000C2
00C3  6E            LD L,(HL)                    ; archivo 0x000000C3
00C4  29            ADD HL,HL                    ; archivo 0x000000C4
00C5  223B03        LD (&033B),HL                ; archivo 0x000000C5
00C8  00            NOP                          ; archivo 0x000000C8
00C9  00            NOP                          ; archivo 0x000000C9
00CA  E9            JP (HL)                      ; archivo 0x000000CA
00CB  00            NOP                          ; archivo 0x000000CB
00CC  2000          JR NZ,&00CE                  ; archivo 0x000000CC
00CE  3C            INC A                        ; archivo 0x000000CE
00CF  00            NOP                          ; archivo 0x000000CF
00D0  A1            AND C                        ; archivo 0x000000D0
00D1  2003          JR NZ,&00D6                  ; archivo 0x000000D1
00D3  00            NOP                          ; archivo 0x000000D3
00D4  00            NOP                          ; archivo 0x000000D4
00D5  E9            JP (HL)                      ; archivo 0x000000D5
00D6  EF            RST &28                      ; archivo 0x000000D6
00D7  226F22        LD (&226F),HL                ; archivo 0x000000D7
00DA  20FC          JR NZ,&00D8                  ; archivo 0x000000DA
00DC  2003          JR NZ,&00E1                  ; archivo 0x000000DC
00DE  00            NOP                          ; archivo 0x000000DE
00DF  00            NOP                          ; archivo 0x000000DF
00E0  E9            JP (HL)                      ; archivo 0x000000E0
00E1  EF            RST &28                      ; archivo 0x000000E1
00E2  224F22        LD (&224F),HL                ; archivo 0x000000E2
00E5  20EB          JR NZ,&00D2                  ; archivo 0x000000E5
00E7  201E          JR NZ,&0107                  ; archivo 0x000000E7
00E9  50            LD D,B                       ; archivo 0x000000E9
00EA  00            NOP                          ; archivo 0x000000EA
00EB  00            NOP                          ; archivo 0x000000EB
00EC  2800          JR Z,&00EE                   ; archivo 0x000000EC
00EE  46            LD B,(HL)                    ; archivo 0x000000EE
00EF  00            NOP                          ; archivo 0x000000EF
00F0  A9            XOR C                        ; archivo 0x000000F0
00F1  2019          JR NZ,&010C                  ; archivo 0x000000F1
00F3  0A            LD A,(BC)                    ; archivo 0x000000F3
00F4  2C            INC L                        ; archivo 0x000000F4
00F5  19            ADD HL,DE                    ; archivo 0x000000F5
00F6  12            LD (DE),A                    ; archivo 0x000000F6
00F7  01A322        LD BC,&22A3                  ; archivo 0x000000F7
00FA  56            LD D,(HL)                    ; archivo 0x000000FA
00FB  69            LD L,C                       ; archivo 0x000000FB
00FC  65            LD H,L                       ; archivo 0x000000FC
00FD  73            LD (HL),E                    ; archivo 0x000000FD
00FE  2069          JR NZ,&0169                  ; archivo 0x000000FE
0100  6E            LD L,(HL)                    ; archivo 0x00000100
0101  66            LD H,(HL)                    ; archivo 0x00000101
0102  69            LD L,C                       ; archivo 0x00000102
0103  6E            LD L,(HL)                    ; archivo 0x00000103
0104  69            LD L,C                       ; archivo 0x00000104
0105  65            LD H,L                       ; archivo 0x00000105
0106  73            LD (HL),E                    ; archivo 0x00000106
0107  2028          JR NZ,&0131                  ; archivo 0x00000107
0109  6F            LD L,A                       ; archivo 0x00000109
010A  2F            CPL                          ; archivo 0x0000010A
010B  6E            LD L,(HL)                    ; archivo 0x0000010B
010C  29            ADD HL,HL                    ; archivo 0x0000010C
010D  223B03        LD (&033B),HL                ; archivo 0x0000010D
0110  00            NOP                          ; archivo 0x00000110
0111  00            NOP                          ; archivo 0x00000111
0112  F600          OR &00                       ; archivo 0x00000112
0114  2B            DEC HL                       ; archivo 0x00000114
0115  00            NOP                          ; archivo 0x00000115
0116  50            LD D,B                       ; archivo 0x00000116
0117  00            NOP                          ; archivo 0x00000117
0118  AD            XOR L                        ; archivo 0x00000118
0119  200E          JR NZ,&0129                  ; archivo 0x00000119
011B  019E20        LD BC,&209E                  ; archivo 0x0000011B
011E  0D            DEC C                        ; archivo 0x0000011E
011F  00            NOP                          ; archivo 0x0000011F
0120  00            NOP                          ; archivo 0x00000120
0121  E9            JP (HL)                      ; archivo 0x00000121
0122  EF            RST &28                      ; archivo 0x00000122
0123  0E20          LD C,&20                     ; archivo 0x00000123
0125  EC2019        CALL PE,&1920                ; archivo 0x00000125
0128  0F            RRCA                         ; archivo 0x00000128
0129  01C320        LD BC,&20C3                  ; archivo 0x00000129
012C  0D            DEC C                        ; archivo 0x0000012C
012D  00            NOP                          ; archivo 0x0000012D
012E  00            NOP                          ; archivo 0x0000012E
012F  E1            POP HL                       ; archivo 0x0000012F
0130  01A220        LD BC,&20A2                  ; archivo 0x00000130
0133  0D            DEC C                        ; archivo 0x00000133
0134  00            NOP                          ; archivo 0x00000134
0135  00            NOP                          ; archivo 0x00000135
0136  E9            JP (HL)                      ; archivo 0x00000136
0137  2C            INC L                        ; archivo 0x00000137
0138  0D            DEC C                        ; archivo 0x00000138
0139  00            NOP                          ; archivo 0x00000139
013A  00            NOP                          ; archivo 0x0000013A
013B  E1            POP HL                       ; archivo 0x0000013B
013C  01B000        LD BC,&00B0                  ; archivo 0x0000013C
013F  1A            LD A,(DE)                    ; archivo 0x0000013F
0140  00            NOP                          ; archivo 0x00000140
0141  5A            LD E,D                       ; archivo 0x00000141
0142  00            NOP                          ; archivo 0x00000142
0143  A8            XOR B                        ; archivo 0x00000143
0144  222168        LD (&6821),HL                ; archivo 0x00000144
0147  65            LD H,L                       ; archivo 0x00000147
0148  61            LD H,C                       ; archivo 0x00000148
0149  64            LD H,H                       ; archivo 0x00000149
014A  69            LD L,C                       ; archivo 0x0000014A
014B  6D            LD L,L                       ; archivo 0x0000014B
014C  61            LD H,C                       ; archivo 0x0000014C
014D  2E70          LD L,&70                     ; archivo 0x0000014D
014F  6F            LD L,A                       ; archivo 0x0000014F
0150  77            LD (HL),A                    ; archivo 0x00000150
0151  220183        LD (&8301),HL                ; archivo 0x00000151
0154  201C          JR NZ,&0172                  ; archivo 0x00000154
0156  00            NOP                          ; archivo 0x00000156
0157  8C            ADC A,H                      ; archivo 0x00000157
0158  00            NOP                          ; archivo 0x00000158
0159  2D            DEC L                        ; archivo 0x00000159
015A  00            NOP                          ; archivo 0x0000015A
015B  64            LD H,H                       ; archivo 0x0000015B
015C  00            NOP                          ; archivo 0x0000015C
015D  8C            ADC A,H                      ; archivo 0x0000015D
015E  2030          JR NZ,&0190                  ; archivo 0x0000015E
0160  2C            INC L                        ; archivo 0x00000160
0161  31332C        LD SP,&2C33                  ; archivo 0x00000161
0164  32362C        LD (&2C36),A                 ; archivo 0x00000164
0167  32342C        LD (&2C34),A                 ; archivo 0x00000167
016A  32352C        LD (&2C35),A                 ; archivo 0x0000016A
016D  39            ADD HL,SP                    ; archivo 0x0000016D
016E  2C            INC L                        ; archivo 0x0000016E
016F  31352C        LD SP,&2C35                  ; archivo 0x0000016F
0172  31312C        LD SP,&2C31                  ; archivo 0x00000172
0175  32302C        LD (&2C30),A                 ; archivo 0x00000175
0178  312C32        LD SP,&322C                  ; archivo 0x00000178
017B  2C            INC L                        ; archivo 0x0000017B
017C  362C          LD (HL),&2C                  ; archivo 0x0000017C
017E  33            INC SP                       ; archivo 0x0000017E
017F  2C            INC L                        ; archivo 0x0000017F
0180  382C          JR C,&01AE                   ; archivo 0x00000180
0182  37            SCF                          ; archivo 0x00000182
0183  2C            INC L                        ; archivo 0x00000183
0184  34            INC (HL)                     ; archivo 0x00000184
0185  00            NOP                          ; archivo 0x00000185
0186  2800          JR Z,&0188                   ; archivo 0x00000186
0188  6E            LD L,(HL)                    ; archivo 0x00000188
0189  00            NOP                          ; archivo 0x00000189
018A  B9            CP C                         ; archivo 0x0000018A
018B  201C          JR NZ,&01A9                  ; archivo 0x0000018B
018D  00            NOP                          ; archivo 0x0000018D
018E  7F            LD A,A                       ; archivo 0x0000018E
018F  2C            INC L                        ; archivo 0x0000018F
0190  1C            INC E                        ; archivo 0x00000190
0191  C40001        CALL NZ,&0100                ; archivo 0x00000191
0194  A8            XOR B                        ; archivo 0x00000194
0195  226865        LD (&6568),HL                ; archivo 0x00000195
0198  61            LD H,C                       ; archivo 0x00000198
0199  64            LD H,H                       ; archivo 0x00000199
019A  2E62          LD L,&62                     ; archivo 0x0000019A
019C  69            LD L,C                       ; archivo 0x0000019C
019D  32222C        LD (&2C22),A                 ; archivo 0x0000019D
01A0  1C            INC E                        ; archivo 0x000001A0
01A1  00            NOP                          ; archivo 0x000001A1
01A2  40            LD B,B                       ; archivo 0x000001A2
01A3  01B920        LD BC,&20B9                  ; archivo 0x000001A3
01A6  1C            INC E                        ; archivo 0x000001A6
01A7  00            NOP                          ; archivo 0x000001A7
01A8  7F            LD A,A                       ; archivo 0x000001A8
01A9  2C            INC L                        ; archivo 0x000001A9
01AA  1C            INC E                        ; archivo 0x000001AA
01AB  C0            RET NZ                       ; archivo 0x000001AB
01AC  00            NOP                          ; archivo 0x000001AC
01AD  00            NOP                          ; archivo 0x000001AD
01AE  15            DEC D                        ; archivo 0x000001AE
01AF  00            NOP                          ; archivo 0x000001AF
01B0  78            LD A,B                       ; archivo 0x000001B0
01B1  00            NOP                          ; archivo 0x000001B1
01B2  A8            XOR B                        ; archivo 0x000001B2
01B3  222168        LD (&6821),HL                ; archivo 0x000001B3
01B6  65            LD H,L                       ; archivo 0x000001B6
01B7  61            LD H,C                       ; archivo 0x000001B7
01B8  64            LD H,H                       ; archivo 0x000001B8
01B9  2E62          LD L,&62                     ; archivo 0x000001B9
01BB  69            LD L,C                       ; archivo 0x000001BB
01BC  31222C        LD SP,&2C22                  ; archivo 0x000001BC
01BF  1C            INC E                        ; archivo 0x000001BF
01C0  00            NOP                          ; archivo 0x000001C0
01C1  0600          LD B,&00                     ; archivo 0x000001C1
01C3  3600          LD (HL),&00                  ; archivo 0x000001C3
01C5  82            ADD A,D                      ; archivo 0x000001C5
01C6  00            NOP                          ; archivo 0x000001C6
01C7  A1            AND C                        ; archivo 0x000001C7
01C8  2003          JR NZ,&01CD                  ; archivo 0x000001C8
01CA  00            NOP                          ; archivo 0x000001CA
01CB  00            NOP                          ; archivo 0x000001CB
01CC  76            HALT                         ; archivo 0x000001CC
01CD  E9            JP (HL)                      ; archivo 0x000001CD
01CE  EF            RST &28                      ; archivo 0x000001CE
01CF  226F22        LD (&226F),HL                ; archivo 0x000001CF
01D2  20FC          JR NZ,&01D0                  ; archivo 0x000001D2
01D4  2003          JR NZ,&01D9                  ; archivo 0x000001D4
01D6  00            NOP                          ; archivo 0x000001D6
01D7  00            NOP                          ; archivo 0x000001D7
01D8  76            HALT                         ; archivo 0x000001D8
01D9  E9            JP (HL)                      ; archivo 0x000001D9
01DA  EF            RST &28                      ; archivo 0x000001DA
01DB  224F22        LD (&224F),HL                ; archivo 0x000001DB
01DE  20EB          JR NZ,&01CB                  ; archivo 0x000001DE
01E0  20BE          JR NZ,&01A0                  ; archivo 0x000001E0
01E2  201C          JR NZ,&0200                  ; archivo 0x000001E2
01E4  3E06          LD A,&06                     ; archivo 0x000001E4
01E6  2C            INC L                        ; archivo 0x000001E6
01E7  0E01          LD C,&01                     ; archivo 0x000001E7
01E9  BE            CP (HL)                      ; archivo 0x000001E9
01EA  201C          JR NZ,&0208                  ; archivo 0x000001EA
01EC  3F            CCF                          ; archivo 0x000001EC
01ED  062C          LD B,&2C                     ; archivo 0x000001ED
01EF  0E01          LD C,&01                     ; archivo 0x000001EF
01F1  BE            CP (HL)                      ; archivo 0x000001F1
01F2  201C          JR NZ,&0210                  ; archivo 0x000001F2
01F4  40            LD B,B                       ; archivo 0x000001F4
01F5  062C          LD B,&2C                     ; archivo 0x000001F5
01F7  0E00          LD C,&00                     ; archivo 0x000001F7
01F9  2800          JR Z,&01FB                   ; archivo 0x000001F9
01FB  8C            ADC A,H                      ; archivo 0x000001FB
01FC  00            NOP                          ; archivo 0x000001FC
01FD  A1            AND C                        ; archivo 0x000001FD
01FE  2003          JR NZ,&0203                  ; archivo 0x000001FE
0200  00            NOP                          ; archivo 0x00000200
0201  00            NOP                          ; archivo 0x00000201
0202  65            LD H,L                       ; archivo 0x00000202
0203  F1            POP AF                       ; archivo 0x00000203
0204  EF            RST &28                      ; archivo 0x00000204
0205  226F22        LD (&226F),HL                ; archivo 0x00000205
0208  20FC          JR NZ,&0206                  ; archivo 0x00000208
020A  2003          JR NZ,&020F                  ; archivo 0x0000020A
020C  00            NOP                          ; archivo 0x0000020C
020D  00            NOP                          ; archivo 0x0000020D
020E  65            LD H,L                       ; archivo 0x0000020E
020F  F1            POP AF                       ; archivo 0x0000020F
0210  EF            RST &28                      ; archivo 0x00000210
0211  224F22        LD (&224F),HL                ; archivo 0x00000211
0214  20EB          JR NZ,&0201                  ; archivo 0x00000214
0216  20BE          JR NZ,&01D6                  ; archivo 0x00000216
0218  201C          JR NZ,&0236                  ; archivo 0x00000218
021A  72            LD (HL),D                    ; archivo 0x0000021A
021B  29            ADD HL,HL                    ; archivo 0x0000021B
021C  2C            INC L                        ; archivo 0x0000021C
021D  1C            INC E                        ; archivo 0x0000021D
021E  FF            RST &38                      ; archivo 0x0000021E
021F  00            NOP                          ; archivo 0x0000021F
0220  00            NOP                          ; archivo 0x00000220
0221  2800          JR Z,&0223                   ; archivo 0x00000221
0223  96            SUB (HL)                     ; archivo 0x00000223
0224  00            NOP                          ; archivo 0x00000224
0225  A1            AND C                        ; archivo 0x00000225
0226  2003          JR NZ,&022B                  ; archivo 0x00000226
0228  00            NOP                          ; archivo 0x00000228
0229  00            NOP                          ; archivo 0x00000229
022A  74            LD (HL),H                    ; archivo 0x0000022A
022B  E9            JP (HL)                      ; archivo 0x0000022B
022C  EF            RST &28                      ; archivo 0x0000022C
022D  226F22        LD (&226F),HL                ; archivo 0x0000022D
0230  20FC          JR NZ,&022E                  ; archivo 0x00000230
0232  2003          JR NZ,&0237                  ; archivo 0x00000232
0234  00            NOP                          ; archivo 0x00000234
0235  00            NOP                          ; archivo 0x00000235
0236  74            LD (HL),H                    ; archivo 0x00000236
0237  E9            JP (HL)                      ; archivo 0x00000237
0238  EF            RST &28                      ; archivo 0x00000238
0239  224F22        LD (&224F),HL                ; archivo 0x00000239
023C  20EB          JR NZ,&0229                  ; archivo 0x0000023C
023E  20BE          JR NZ,&01FE                  ; archivo 0x0000023E
0240  201C          JR NZ,&025E                  ; archivo 0x00000240
0242  322B2C        LD (&2C2B),A                 ; archivo 0x00000242
0245  1C            INC E                        ; archivo 0x00000245
0246  1800          JR &0248                     ; archivo 0x00000246
0248  00            NOP                          ; archivo 0x00000248
0249  2600          LD H,&00                     ; archivo 0x00000249
024B  A0            AND B                        ; archivo 0x0000024B
024C  00            NOP                          ; archivo 0x0000024C
024D  A1            AND C                        ; archivo 0x0000024D
024E  2003          JR NZ,&0253                  ; archivo 0x0000024E
0250  00            NOP                          ; archivo 0x00000250
0251  00            NOP                          ; archivo 0x00000251
0252  E9            JP (HL)                      ; archivo 0x00000252
0253  EF            RST &28                      ; archivo 0x00000253
0254  226F22        LD (&226F),HL                ; archivo 0x00000254
0257  20FC          JR NZ,&0255                  ; archivo 0x00000257
0259  2003          JR NZ,&025E                  ; archivo 0x00000259
025B  00            NOP                          ; archivo 0x0000025B
025C  00            NOP                          ; archivo 0x0000025C
025D  E9            JP (HL)                      ; archivo 0x0000025D
025E  EF            RST &28                      ; archivo 0x0000025E
025F  224F22        LD (&224F),HL                ; archivo 0x0000025F
0262  20EB          JR NZ,&024F                  ; archivo 0x00000262
0264  20BE          JR NZ,&0224                  ; archivo 0x00000264
0266  201C          JR NZ,&0284                  ; archivo 0x00000266
0268  45            LD B,L                       ; archivo 0x00000268
0269  4C            LD C,H                       ; archivo 0x00000269
026A  2C            INC L                        ; archivo 0x0000026A
026B  1C            INC E                        ; archivo 0x0000026B
026C  C9            RET                          ; archivo 0x0000026C
026D  00            NOP                          ; archivo 0x0000026D
026E  00            NOP                          ; archivo 0x0000026E
026F  24            INC H                        ; archivo 0x0000026F
0270  00            NOP                          ; archivo 0x00000270
0271  AA            XOR D                        ; archivo 0x00000271
0272  00            NOP                          ; archivo 0x00000272
0273  A1            AND C                        ; archivo 0x00000273
0274  2003          JR NZ,&0279                  ; archivo 0x00000274
0276  00            NOP                          ; archivo 0x00000276
0277  00            NOP                          ; archivo 0x00000277
0278  F6EF          OR &EF                       ; archivo 0x00000278
027A  226F22        LD (&226F),HL                ; archivo 0x0000027A
027D  20FC          JR NZ,&027B                  ; archivo 0x0000027D
027F  2003          JR NZ,&0284                  ; archivo 0x0000027F
0281  00            NOP                          ; archivo 0x00000281
0282  00            NOP                          ; archivo 0x00000282
0283  F6EF          OR &EF                       ; archivo 0x00000283
0285  224F22        LD (&224F),HL                ; archivo 0x00000285
0288  20EB          JR NZ,&0275                  ; archivo 0x00000288
028A  20BE          JR NZ,&024A                  ; archivo 0x0000028A
028C  201C          JR NZ,&02AA                  ; archivo 0x0000028C
028E  C5            PUSH BC                      ; archivo 0x0000028E
028F  2B            DEC HL                       ; archivo 0x0000028F
0290  2C            INC L                        ; archivo 0x00000290
0291  0E00          LD C,&00                     ; archivo 0x00000291
0293  3C            INC A                        ; archivo 0x00000293
0294  00            NOP                          ; archivo 0x00000294
0295  B4            OR H                         ; archivo 0x00000295
0296  00            NOP                          ; archivo 0x00000296
0297  9E            SBC A,(HL)                   ; archivo 0x00000297
0298  200D          JR NZ,&02A7                  ; archivo 0x00000298
029A  00            NOP                          ; archivo 0x0000029A
029B  00            NOP                          ; archivo 0x0000029B
029C  EEEF          XOR &EF                      ; archivo 0x0000029C
029E  1C            INC E                        ; archivo 0x0000029E
029F  80            ADD A,B                      ; archivo 0x0000029F
02A0  BE            CP (HL)                      ; archivo 0x000002A0
02A1  20EC          JR NZ,&028F                  ; archivo 0x000002A1
02A3  201C          JR NZ,&02C1                  ; archivo 0x000002A3
02A5  A3            AND E                        ; archivo 0x000002A5
02A6  BE            CP (HL)                      ; archivo 0x000002A6
02A7  01C320        LD BC,&20C3                  ; archivo 0x000002A7
02AA  03            INC BC                       ; archivo 0x000002AA
02AB  00            NOP                          ; archivo 0x000002AB
02AC  00            NOP                          ; archivo 0x000002AC
02AD  E1            POP HL                       ; archivo 0x000002AD
02AE  010D00        LD BC,&000D                  ; archivo 0x000002AE
02B1  00            NOP                          ; archivo 0x000002B1
02B2  E1            POP HL                       ; archivo 0x000002B2
02B3  EF            RST &28                      ; archivo 0x000002B3
02B4  FF            RST &38                      ; archivo 0x000002B4
02B5  1D            DEC E                        ; archivo 0x000002B5
02B6  2822          JR Z,&02DA                   ; archivo 0x000002B6
02B8  2622          LD H,&22                     ; archivo 0x000002B8
02BA  F40300        CALL P,&0003                 ; archivo 0x000002BA
02BD  00            NOP                          ; archivo 0x000002BD
02BE  E1            POP HL                       ; archivo 0x000002BE
02BF  29            ADD HL,HL                    ; archivo 0x000002BF
02C0  01BE20        LD BC,&20BE                  ; archivo 0x000002C0
02C3  0D            DEC C                        ; archivo 0x000002C3
02C4  00            NOP                          ; archivo 0x000002C4
02C5  00            NOP                          ; archivo 0x000002C5
02C6  EE2C          XOR &2C                      ; archivo 0x000002C6
02C8  0D            DEC C                        ; archivo 0x000002C8
02C9  00            NOP                          ; archivo 0x000002C9
02CA  00            NOP                          ; archivo 0x000002CA
02CB  E1            POP HL                       ; archivo 0x000002CB
02CC  01B000        LD BC,&00B0                  ; archivo 0x000002CC
02CF  2100BE        LD HL,&BE00                  ; archivo 0x000002CF
02D2  00            NOP                          ; archivo 0x000002D2
02D3  AD            XOR L                        ; archivo 0x000002D3
02D4  200E          JR NZ,&02E4                  ; archivo 0x000002D4
02D6  019E20        LD BC,&209E                  ; archivo 0x000002D6
02D9  0D            DEC C                        ; archivo 0x000002D9
02DA  00            NOP                          ; archivo 0x000002DA
02DB  00            NOP                          ; archivo 0x000002DB
02DC  EEEF          XOR &EF                      ; archivo 0x000002DC
02DE  0E20          LD C,&20                     ; archivo 0x000002DE
02E0  EC2019        CALL PE,&1920                ; archivo 0x000002E0
02E3  0F            RRCA                         ; archivo 0x000002E3
02E4  01A220        LD BC,&20A2                  ; archivo 0x000002E4
02E7  0D            DEC C                        ; archivo 0x000002E7
02E8  00            NOP                          ; archivo 0x000002E8
02E9  00            NOP                          ; archivo 0x000002E9
02EA  EE2C          XOR &2C                      ; archivo 0x000002EA
02EC  0E01          LD C,&01                     ; archivo 0x000002EC
02EE  B0            OR B                         ; archivo 0x000002EE
02EF  00            NOP                          ; archivo 0x000002EF
02F0  3600          LD (HL),&00                  ; archivo 0x000002F0
02F2  C8            RET Z                        ; archivo 0x000002F2
02F3  00            NOP                          ; archivo 0x000002F3
02F4  8C            ADC A,H                      ; archivo 0x000002F4
02F5  2066          JR NZ,&035D                  ; archivo 0x000002F5
02F7  33            INC SP                       ; archivo 0x000002F7
02F8  2C            INC L                        ; archivo 0x000002F8
02F9  32312C        LD (&2C31),A                 ; archivo 0x000002F9
02FC  3030          JR NC,&032E                  ; archivo 0x000002FC
02FE  2C            INC L                        ; archivo 0x000002FE
02FF  3036          JR NC,&0337                  ; archivo 0x000002FF
0301  2C            INC L                        ; archivo 0x00000301
0302  31312C        LD SP,&2C31                  ; archivo 0x00000302
0305  3030          JR NC,&0337                  ; archivo 0x00000305
0307  2C            INC L                        ; archivo 0x00000307
0308  3031          JR NC,&033B                  ; archivo 0x00000308
030A  2C            INC L                        ; archivo 0x0000030A
030B  3031          JR NC,&033E                  ; archivo 0x0000030B
030D  2C            INC L                        ; archivo 0x0000030D
030E  3030          JR NC,&0340                  ; archivo 0x0000030E
0310  2C            INC L                        ; archivo 0x00000310
0311  39            ADD HL,SP                    ; archivo 0x00000311
0312  66            LD H,(HL)                    ; archivo 0x00000312
0313  2C            INC L                        ; archivo 0x00000313
0314  65            LD H,L                       ; archivo 0x00000314
0315  64            LD H,H                       ; archivo 0x00000315
0316  2C            INC L                        ; archivo 0x00000316
0317  62            LD H,D                       ; archivo 0x00000317
0318  302C          JR NC,&0346                  ; archivo 0x00000318
031A  3031          JR NC,&034D                  ; archivo 0x0000031A
031C  2C            INC L                        ; archivo 0x0000031C
031D  63            LD H,E                       ; archivo 0x0000031D
031E  34            INC (HL)                     ; archivo 0x0000031E
031F  2C            INC L                        ; archivo 0x0000031F
0320  37            SCF                          ; archivo 0x00000320
0321  66            LD H,(HL)                    ; archivo 0x00000321
0322  2C            INC L                        ; archivo 0x00000322
0323  65            LD H,L                       ; archivo 0x00000323
0324  64            LD H,H                       ; archivo 0x00000324
0325  00            NOP                          ; archivo 0x00000325
0326  3600          LD (HL),&00                  ; archivo 0x00000326
0328  D2008C        JP NC,&8C00                  ; archivo 0x00000328
032B  2034          JR NZ,&0361                  ; archivo 0x0000032B
032D  39            ADD HL,SP                    ; archivo 0x0000032D
032E  2C            INC L                        ; archivo 0x0000032E
032F  32312C        LD (&2C31),A                 ; archivo 0x0000032F
0332  3030          JR NC,&0364                  ; archivo 0x00000332
0334  2C            INC L                        ; archivo 0x00000334
0335  34            INC (HL)                     ; archivo 0x00000335
0336  302C          JR NC,&0364                  ; archivo 0x00000336
0338  31312C        LD SP,&2C31                  ; archivo 0x00000338
033B  3030          JR NC,&036D                  ; archivo 0x0000033B
033D  2C            INC L                        ; archivo 0x0000033D
033E  61            LD H,C                       ; archivo 0x0000033E
033F  302C          JR NC,&036D                  ; archivo 0x0000033F
0341  3031          JR NC,&0374                  ; archivo 0x00000341
0343  2C            INC L                        ; archivo 0x00000343
0344  63            LD H,E                       ; archivo 0x00000344
0345  302C          JR NC,&0373                  ; archivo 0x00000345
0347  3064          JR NC,&03AD                  ; archivo 0x00000347
0349  2C            INC L                        ; archivo 0x00000349
034A  65            LD H,L                       ; archivo 0x0000034A
034B  64            LD H,H                       ; archivo 0x0000034B
034C  2C            INC L                        ; archivo 0x0000034C
034D  62            LD H,D                       ; archivo 0x0000034D
034E  302C          JR NC,&037C                  ; archivo 0x0000034E
0350  3031          JR NC,&0383                  ; archivo 0x00000350
0352  2C            INC L                        ; archivo 0x00000352
0353  63            LD H,E                       ; archivo 0x00000353
0354  302C          JR NC,&0382                  ; archivo 0x00000354
0356  37            SCF                          ; archivo 0x00000356
0357  66            LD H,(HL)                    ; archivo 0x00000357
0358  2C            INC L                        ; archivo 0x00000358
0359  65            LD H,L                       ; archivo 0x00000359
035A  64            LD H,H                       ; archivo 0x0000035A
035B  00            NOP                          ; archivo 0x0000035B
035C  12            LD (DE),A                    ; archivo 0x0000035C
035D  00            NOP                          ; archivo 0x0000035D
035E  DC008C        CALL C,&8C00                 ; archivo 0x0000035E
0361  2034          JR NZ,&0397                  ; archivo 0x00000361
0363  39            ADD HL,SP                    ; archivo 0x00000363
0364  2C            INC L                        ; archivo 0x00000364
0365  63            LD H,E                       ; archivo 0x00000365
0366  33            INC SP                       ; archivo 0x00000366
0367  2C            INC L                        ; archivo 0x00000367
0368  3030          JR NC,&039A                  ; archivo 0x00000368
036A  2C            INC L                        ; archivo 0x0000036A
036B  3031          JR NC,&039E                  ; archivo 0x0000036B
036D  00            NOP                          ; archivo 0x0000036D
036E  0A            LD A,(BC)                    ; archivo 0x0000036E
036F  00            NOP                          ; archivo 0x0000036F
0370  E600          AND &00                      ; archivo 0x00000370
0372  83            ADD A,E                      ; archivo 0x00000372
0373  201C          JR NZ,&0391                  ; archivo 0x00000373
0375  80            ADD A,B                      ; archivo 0x00000375
0376  BE            CP (HL)                      ; archivo 0x00000376
0377  00            NOP                          ; archivo 0x00000377
0378  00            NOP                          ; archivo 0x00000378
0379  00            NOP                          ; archivo 0x00000379
