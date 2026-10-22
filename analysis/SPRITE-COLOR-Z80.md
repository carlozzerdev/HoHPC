# Composición cromática de sprites

Las dos mitades son datos y máscara, pero NO se deben convertir en un único color de tinta con transparencia.

35B3 devuelve DE apuntando a datos y HL a máscara. 149A–14A5 compone ambos buffers distintos:

* buffer67 = (buffer67 AND mask) OR data
* buffer68 = (buffer68 OR data) AND mask

05E5–05F6 convierte ambos buffers a bytes de pantalla mode1 usando LUT6600 (swap de nibbles generado04EB). Buffer67 contribuye al bit1 del índice CPC, buffer68 al bit0.

Por tanto composición por píxel: `newLow = (oldLow | data) & mask`, `newHigh = (oldHigh & mask) | data`. Tabla para sprites sobre cualquier fondo:

|data|mask|Resultado|
|---|---|---|
|0|0|Tinta0 opaca|
|0|1|Conservar fondo/transparente|
|1|0|Tinta2 opaca|
|1|1|Tinta3 opaca|

Conversión RGBA exacta: alpha=`data || !mask`; índice=`(data ? 2 : 0) | (data && mask ? 1 : 0)`. La fórmula `data | (!mask << 1)` inventaría tinta2 donde el original pinta negro y cambiaría colores restantes.

Tinta1 del original puede aparecer en el fondo o regiones que se preservan; no aparece como nuevo color opaco de este blitter de personaje. Paredes usan sus dos planos directamente y sí pueden proporcionar las cuatro tintas.

Paletas: rutina052C/054A selecciona cuatro bytes en0554+id*4. Se exportan8 entradas en `palettes-cpc.json`, códigos hardware GateArray sin prefijo40 y conversión RGB empírica de CPCEC.C:147; el borde usa también la tinta0. No confundir estos códigos hardware con índices firmware0–26.

Paredes: 1A80 escribe directamente en buffers, sin máscara/transparencia del blitter149A. DE empieza en6800 (1735/174B). Fila(a,b,c,d): bit0 CPC usa(a,d), bit1 usa(b,c). Por píxel `color = plane0Bit | (plane1Bit << 1)`, siempre opaco. Por tanto paredes sí pintan tinta1 azul y requieren recuperar ambos planos.
