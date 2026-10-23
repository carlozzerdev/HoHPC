# Detalles visuales recuperados del disco

Evidencia estática local: `relocated.bin`, regenerado desde el DSK por
`tools/recover.py`, y `analysis/original/z80.asm`. No se ha ejecutado Z80.
Las instrucciones de los rangos citados se compararon byte a byte con memoria.

## Suelos

`19D6–19E7` selecciona `B4B0 + 48 * floor`. `19EA–1A04` usa patrones
monocromos de 16 filas y 2 bytes por fila; las filas adicionales repiten
las primeras para admitir desplazamiento. `1A04` selecciona desplazamiento
00/10 hex según bit7 del segmento: las columnas alternas desplazan el patrón
8 filas. Se exportan ambas fases como `floorTiles`
(SpriteData, 32×16, pixels 0/1). Son patrones en coordenadas de pantalla;
no deben estirarse individualmente sobre cada rombo del suelo.

`2CB7–2CE3` distingue floor 6 (contacto peligroso, llama `4738`) de floor 7
(transición vertical). `2CF6–2D02` comprueba Z=C0 para contacto al nivel
del suelo; no se necesita caer por debajo del suelo para morir en floor 6.
La protección temporal se consulta dentro de `4738`.

## Puertas

`20DE` y `045D` seleccionan estilo: si es >=3 se resta 1, y se multiplica
por 2. Las dos mitades usan los sprites consecutivos resultantes.
`2164–2194` sitúa sus coordenadas paralelas en 36 y 44 unidades nativas
(3 y 4 en las coordenadas del port, cuya transformación resta 12).
La coordenada perpendicular es bound+4 para máximos, bound−4 para mínimos.
El código actual del port ya realizaba correctamente estas posiciones.

Flags originales `1EB1–1EB8`: `27 26 17 15 05 04 36 34`. El bit 4
indica reflejo; ambas mitades se reflejan en lados 1 y 3.
`2195–21BE` añade sprite 54 hex (84 decimal) una unidad de altura debajo
de cada mitad elevada. No lo añade en puertas al nivel del suelo (Z=C0).

La apertura en paredes usa selector 4 en `3D41–3D78`, y la ruta especial
`195B–197A` consume gráfico generado dinámicamente en B898 (`16EE`).
Este gráfico no existe como sprite estático en el banco recuperado. Un recorte
negro nativo de la apertura es una adaptación, no un bitmap extraído.

## Muerte y recogida

Muerte: `4738–4770` establece contador 12 y sonido C6; `26A8` lo decrementa
y `2739–2778` aplica la secuencia `24ED–24F8`:
`24 A4 A5 25 A5 A6 26 26 A6 A6 26 26`.
Los valores con bit 7 reflejan horizontalmente el sprite base; no son IDs nuevos.
Teletransporte de entrada: `24FB–2502`, `26 A6 26 A6 A5 25 24 A5`.
Se exportan como `deathAnimation` y `teleportInAnimation`.

`4773–478C` cambia el coleccionable al comportamiento 17 (desaparecer),
contador 8, flags 80; sigue animación estándar y se elimina al terminar.
`4D6D–4D95` comparte esa desaparición para otros contactos; no debe
confundirse con llevar un objeto en la bolsa. La bolsa (`2A8A` y siguientes)
retira el objeto inmediatamente mediante `45F3` y `44CA`.
No se ha encontrado evidencia de una explosión adicional para recoger objetos.
El efecto visual nativo debe conservar brevemente el sprite y su animación.
