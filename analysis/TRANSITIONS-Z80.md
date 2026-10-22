# Puertas y teletransportes

Análisis estático sobre los bytes del DSK suministrado; implementación nativa.

## Ejes e identificadores

`018D–01B7` modifica el nibble alto del byte XY de habitación al cruzar un
límite físico X y el nibble bajo al cruzar un límite físico Y. No se debe usar
directamente `(gridX+dx,gridY+dy)`. Los campos de habitación gridX/gridY del JSON
conservan la convención de nibbles; no se cambió el formato para ocultar esta diferencia.

Orden de las cuatro salidas por `20DE–211E`: Y máximo, X máximo, Y mínimo,
X mínimo. Solo códigos 2–7 son puertas visibles; altura nativa=código−2.
`2B76–2B8A` lee la altura de entrada de la sala destino, que puede diferir de
la altura de la puerta de salida. `2BA2–2BEF` sitúa el centro tres unidades
originales dentro del límite opuesto.

El grafo corregido contiene **482 salidas visibles**, todas con destino presente
y salida de retorno visible. Con la correspondencia previa de ejes solo se
encontraban 137 destinos. Las alturas de entrada y salida difieren en 292 enlaces;
no se debe conservar incondicionalmente la altura de la habitación anterior.

## Teletransportes

`2A41–2A48` comprueba el sprite del soporte enmascarado con7F; sprite57(hex),
87(decimal), dirige a `2A7F`, que carga2486=12 y2487=8. `256E–2578` y
`2692–26A5` consumen los contadores. Al terminar salida establece4658=7.

`018D–01BF` distingue este estado y llama346A. `346A–348F` busca en una lista
de parejas de palabras little-endian en3490–34E7. Hay22 parejas, todos los
identificadores existen en las301 habitaciones recuperadas.

La búsqueda es secuencial y simétrica dentro de cada pareja, pero algunas salas
aparecen más de una vez. Por ejemplo BA70 participa en dos parejas. El primer
acierto gana: no convertirla en un diccionario que sobrescriba destinos repetidos.

`2BCA–2BDB` conserva XY y, para un personaje separado, Z. La rama de personaje
unido contiene un ajuste especial de Z relacionado con236E que aún debe
contrastarse; el motor actual conserva el estado compuesto y sus dos alturas.

Pruebas: transición físicaY 8A40→8940, conservación del contexto del otro
personaje, búsqueda de pareja, contador12/8 y conservación de coordenadas.
No se han contrastado todavía todas las animaciones ni recorridos de juego.
