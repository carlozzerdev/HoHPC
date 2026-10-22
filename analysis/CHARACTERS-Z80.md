# Personajes: evidencia estática Z80 del disco CPC

Direcciones hexadecimales sobre `relocated.bin`. La lectura lineal de BINSPY mezcla datos con instrucciones: las tablas siguientes se verifican contra bytes. No se ejecutó la CPU ni una partida durante esta investigación. Los nombres Head/Heels se deducen de sus capacidades características; las estructuras y reglas sí están confirmadas por instrucciones.

## Estado y comienzo

`2484`: máscara activa, bit0 estructura `24B0` (Heels, rápido/salto corto), bit1 estructura `24C2` (Head, lento/salto alto), 3 unidos. Cada estructura ocupa 18 bytes. Offsets +5 X, +6 Y, +7 Z (crece hacia abajo), +8 sprite, +11 flags de movimiento, +12 máscara de movimientos permitidos, +13/+14 referencia de soporte.

`22ED–2321` inicializa habitación `8940`, máscara1, y después `8A40`, máscara2. `2B48–2C2F`, estado de entrada4658=0, copia `2C54: 28 28 C0` a XYZ, por tanto ambos comienzan en (40,40,192). El suelo normal es C0 (`2C6E`). `22F7–22FE` fuerza sprites18 y1F.

## Ciclo lógico

`01C4–01CF` espera0116=0 y recarga4. ISR `04BB–04EA` consulta VSYNC en PPI F5 y usa divisor6; rama04CD llama sonido y decrementa0116 (`04DB–04E2`). Resultado nominal: cuatro cuadros PAL por ciclo lógico, unos80ms/12.5Hz, sujeto a ralentización por trabajo de render. El dato confirmado es cuatro cuadros: 12.5Hz presupone referencia PAL50Hz. `042B` alterna bit7 de0110 y llama2551 una vez por ciclo.

## Movimiento y dirección

`440F` traduce nibble entrada usando441A: `FF 00 04 FF 06 07 05 06 02 01 03 02 FF 00 04 FF`. FF significa dirección no válida. Tabla443A:

| Dir | dx | dy | Flags bajos |
| --- | --- | --- | --- |
|0|-1|0|0D|
|1|-1|-1|09|
|2|0|-1|0B|
|3|1|-1|0A|
|4|1|0|0E|
|5|1|1|06|
|6|0|1|07|
|7|-1|1|05|

`4455` sustituye nibble bajo+11 y suma dx/dy. `2978–29C6` consulta colisión465C antes del primer paso. Cambio de dirección/no movimiento establece2491=2 (`2A0A`); `29C7` consume este contador antes del segundo paso. `29D0–2A07` permite segundo paso a Heels, o Head con bonus247C distinto de0, comprobando colisión otra vez. No hay normalización diagonal.

## Salto y caída

`2A10–2A7C`: requiere24AC=0 y botón de salto activo (bit0 de0112 a0). Si soporte+13/+14 existe examina sus flags y sprite: tipos2B/2C incrementan C, tipo57 dispara transporte especial2486/2487=0C/08. Head (máscara con bit1) incrementa C. Heels comprueba bonus mediante4143 con A=1 y lo incrementa si hay bonus. Contador ascenso248F =4+4*C, sustituyendo12 por10. Base Heels4ticks/Head8ticks. Al saltar Heels establece2483=7, usado posteriormente para mantener dirección en aire.

`27AF–27DB` consume un tick ascenso y resta normalmente2 unidades Z. Primero comprueba colisión ascendente2E31; cuando bloqueado o por condiciones de contacto modifica flags y puede reducir el ascenso. No es una parábola continua.

Caída `2902–2955`: C=0 o1 según estado de soporte y248E. Con Head/máscara bit1 incrementa Z una vez. Con Heels y C=1 incrementa Z dos veces; con C=0 una. Por tanto la descripción anterior «Head cae más rápido» sería incorrecta: **Heels cae hasta2/tick y Head1/tick**. Estado de suelo2C64 compara Z con suelo normalC0 o puertas/soporte. Mantener estas unidades discretas en el motor antes de añadir interpolación visual.

## Animación y reflejo

| Base contador | Secuencia terminada en00 | Personaje |
| --- | --- | --- |
|24D4|18 19 18 1A|Heels|
|24DA|1B 1C 1B 1D|Heels|
|24E0|1E 1F 1E 20|Head|
|24E6|21 22 21 23|Head|

`446F` incrementa contador y obtiene frame; al encontrar00 vuelve a contador1/primerframe. `288C–28B3` elige D4/E0 si bit2 de dirección convertida es0, DA/E6 si es1. `2964` pone flags+4.bit4 si dirección.bit1=0.

Reflejo CONFIRMADO: `1E39–1E4F` convierte flags+4.bit4 en80 y lo XOR con sprite+8; el sprite efectivo tiene bit7 de reflejo horizontal. `3539` genera LUT6900 de inversión de8bits. `36BE` consulta cache de orientación70E0 y `3660` invierte bits e intercambia bytes exteriores de filas3bytes; `369C` hace equivalente con4bytes. El original muta gráficos para cachear orientación. El port debe conservar gráficos base y usar `FlipX = ((sprite ^ (flags4.bit4 ? 0x80 : 0)) & 0x80) != 0`.

## Unión y separación

Botón cambio detectado por flanco0114.bit0 en0276; rechazado mientras24AC,4659,2486 o2487 sean no0 o algún contador de vidas2480/2481 sea0. `0292` guarda dirección actual en0117/0118 según máscara.

`02A8–02D3` intenta unión si máscara actual distinta de3 y2485!=0 (ambos presentes): diferencia X e Y en[-1,+1], y `Z_Heels - 6 == Z_Head`. Si XY ya coinciden, pasa directamente máscara3 en0307. Si hay desplazamiento de1, codifica ajuste en flags de Head+12 (`02D5–02F9`) y aplaza unión; no teletransportar arbitrariamente los personajes a distancia.

Si no cumple condiciones, `02FC` alterna0114.bit2 y elige máscara1/2. Si estaban unidos, esta misma rama los separa seleccionando uno. `032F` actualiza estados+10, Head+10 y flags: al unir activa Heels.flags4.bit3 y Head.flags9.bit1. `39D6` copia geometría y posición de la parte base a la superior restando6 a Z. La entidad compuesta sigue operando sobre24B0, pero máscara3 activa velocidad Heels y salto Head.

## Transporte de objeto

`2A8A` usa flanco0113.bit0 y exige247B.bit0 (habilidad), máscara con bit0 (Heels), y posteriormente24AC=0 para soltar. `2497/2498` guarda referencia del objeto transportado. Si no lleva objeto, `2E02` busca objeto elegible; al éxito lo retira mediante45F3 y44CA. Si ya lleva objeto, comprueba tres posiciones bajando Z dos unidades cada vez con2E31; si alguna colisiona cancela y restaura Z. En éxito copia XY del personaje al objeto, conserva Z inicial en+7 y lo inserta44FE, borra referencia2497 y refresca icono. Predicados completos de elegibilidad2E02/44CA no están descifrados aquí: no presentar cualquier objeto como recogible.

## Transiciones

4658 codifica entrada/salida. `018D` ajusta nibble XY/planta habitación. `2B48` con estados1..4 coloca coordenada perpendicular al umbral3 unidades dentro del límite usando1EC2..1EC5 y alturas1EEE. Estados5/6 manipulan Z;7 aplica transporte especial; rutas superiores usan425E. `2641–268F` guarda contexto del otro personaje antes de cargar nueva habitación. Los contextos individuales y habitación secundaria deben permanecer independientes en la versión de PC.

## Límites

No se ha validado visualmente orientación cardinal absoluta, duración real en hardware, todos los tipos de soporte, animaciones especiales de muerte/teletransporte, o elegibilidad completa de objetos. La estructura del motor final debe permitir sustituir estas hipótesis sin ejecutar opcodes Z80.
