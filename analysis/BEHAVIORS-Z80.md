# Comportamientos, inventario y objetivos

Evidencia estática del binario CPC. Las direcciones son direcciones Z80, no offsets DSK. Los nombres descriptivos indican el efecto demostrado; no todos los enemigos tienen todavía nombre visual confirmado.

## Colectables: tabla independiente

Los objetos coleccionables no forman parte del flujo comprimido de habitaciones. `3ECA–3EE1` busca 52 registros de cuatro bytes en `3DEB`. La construcción se ejecuta en `3EF7–3F4E`, después de crear la geometría de cada habitación. El formato demostrado por los tres RLD por byte de `3EE3–3EF6` es:

- Byte 0: byte bajo de roomID, con bit 0 usado como recogido.
- Byte 1: byte alto de roomID.
- Byte 2: nibble alto Z, nibble bajo clase de colectable.
- Byte 3: nibble alto X, nibble bajo Y.

X/Y/Z usan las mismas conversiones `226E` que los objetos comprimidos. La clase se conserva en IY+17. La conducta generada es 26, rutina `4DCF`. El sprite proviene de `3EBB+clase`. Clase 14 tiene flags cero; las demás flags `60`. La recogida `3F5E` localiza el registro, marca bit 0 y despacha mediante `3F89+2*clase`.

| Clase | Sprite bruto | Efecto/rutina |
|---|---|---|
| 0 | 39 | Bolsa: activa bit 0 de inventario247B, `3FA5` |
| 1 | 40 | Bocina: activa bit 1 de247B, `3FA5` |
| 2 | 41 | Munición: solo activo&2; suma6 al contador2482 y activa bit2 de247B, `3FB4` |
| 3 | 42 | Solo activo&2; añade BCD99 al contador247C, `3FC3` |
| 4 | 42 | Solo activo&1; añade BCD10 al contador247D, `3FCC` |
| 5 | 42 | Añade BCD99 a247E o247F según personaje; ambos si están unidos, `3FD8` |
| 6 | 42 | Añade BCD02 vidas a2480 o2481 según personaje; ambos unidos, `3FDC` |
| 7 | 0 | Entrada sin efecto válido; no crear un pickup con esta clase |
| 8 | 134 | Guarda checkpoint/instantánea, `4025` |
| 9–13 | 47 | Activa bit clase-9 de coronas3DEA, `4014` |
| 14 | 47 | Marcadores especiales; sin entrada de efecto válida |

La suma de contadores se implementa con DAA y saturación BCD99 en `4166–416D`. Cantidades por recurso en `418D`: `99,10,99,99,02,02,06`. Los contadores iniciales vidas2480/2481 son4. `26B2–26D1` resta BCD1 al personaje dañado y termina cuando ambos están a cero.

No se deben convertir clases3–6 en la misma mejora genérica: son contadores diferentes con restricciones diferentes. La atribución visual de3/4 a velocidad/salto y5 a protección requiere contraste del consumo de contadores; el informe conserva sus efectos exactos.

Las cinco coronas normales están representadas por clases9–13. `451D–4522` comprueba3DEA==1F para resolver la victoria. `3EF7–3F0A` también actualiza cinco registros en3EA7 según las coronas. Los últimos cinco registros, clase14, están en habitación8D30 y tienen una función distinta de la recogida normal.

## Bolsa y disparo

`2A8A–2B19` es recoger/dejar. Requiere entrada0113, bolsa bit0 de247B y personaje activo&1. Cuando no lleva objeto busca un cuerpo elegible mediante2E02, conserva su puntero en2497 y lo retira mediante44CA. Al dejarlo comprueba espacio vertical, reconstruye posición/cuerpo y borra2497. Por tanto solo puede llevar un objeto, no un inventario ilimitado.

`25D5–262C` dispara mediante entrada0115. Exige activo==2,247B bits1 y2 presentes, ausencia de acción en24AC y estado24A8==8. Crea un cuerpo/proyectil con sprite19, copia posición24C7, y consume contador recurso6 mediante4143. Si2482 llega a cero borra bit2 de247B (`262E–2639`). Las direcciones de la bocina y bolsa muestran que estos permisos pertenecen a personajes distintos.

## Tabla de comportamientos

El salto de `3764–378B` usa `387F+2*(id-1)`. ID0 no despacha. IY+10 es conducta; IY+16 dirección en ocho sectores; IY+17 estado auxiliar. La tabla siguiente no identifica las especies por el dibujo.

| ID | Rutina | Efecto demostrado |
|---|---|---|
| 1 | 4DDC | Gravedad/colisión y actualización común |
| 2,3,4,5 | 4D46,4D4A,4D4E,4D52 | Inicializan máscara direccional FE,FD,F7,FB y pasan a conducta0 |
| 6 | 4DF3 | Movimiento que invierte dirección (+4)&7 al cambiar trayectoria |
| 7 | 4DF8 | Movimiento que gira (-2)&7 |
| 8 | 4DFD | Movimiento que gira (+2)&7 |
| 9 | 4E11 | Dirección aleatoria impar diferente de anterior |
| 10 | 4C86 | Cuerpo móvil con interacción entre cuerpos; reinicio de estado |
| 11 | 4E02 | Dirección aleatoria entre8 sectores diferente de anterior |
| 12 | 4E07 | Variante aleatoria8, prueba periódica aleatoria1/16 |
| 13 | 4E0C | Variante aleatoria de sectores pares |
| 14 | 4CD6 | Interruptor: alterna bit6 de otras entidades elegibles y bit4 propio |
| 15 | 4E16 | Persecución: selecciona eje dominante hacia personaje, auxiliar4F55 |
| 16 | 4E82 | Movimiento vertical guiado por estado auxiliar |
| 17 | 4D80 | Fase de desaparición; retira cuerpo al completar animación |
| 18 | 4D31 | Inicializa auxiliar52 y pasa a16 |
| 19 | 4D3B | Gravedad condicional según estado bit5 |
| 20 | 4D66 | Activa desaparición según contacto/máscaras |
| 21 | 4DED | Giro(-2)&7, variante de movimiento sin prueba de gravedad inicial |
| 22 | 4DE7 | Giro(+4)&7, variante de movimiento |
| 23 | 4D2E | Inicializa auxiliar90 y pasa a16 |
| 24 | 4D63 | Desaparición condicional con máscaraC0 |
| 25 | 4C76 | Movimiento con respuesta doble a interacción |
| 26 | 4DCF | Actualiza colectable; vuelve a rutina común |
| 27 | 4C18 | Actualización condicionada por bit5 del estado |
| 28 | 4D98 | Plataforma que alterna sprites43/44 y temporizador80 |
| 29 | 4BFB | Consume estado compartido4BEA y mueve cuerpo |
| 30 | 4BEB | Publica máscara de dirección en estado compartido4BEA |
| 31 | 4D5C | Desaparición condicionada por personaje activo&2 |
| 32 | 4C5E | Interacción según dirección relativa al personaje |
| 33 | 4F36 | Persecución solo si distancia X e Y<=24 unidades originales |
| 34 | 4D92 | Retira cuerpo mediante44CA |
| 35 | 4C3F | Variante de interacción con marcador temporal4C3E |
| 36 | 4C29 | Temporizador compartido4C28=96; pasa a conducta25 |
| 37 | 4E1B | Persigue o invierte dirección según progreso de coronas |

`4EF4`, `4F01` y `4F10` eligen respectivamente direcciones pares, impares y cualquiera entre8. `4F1D`, `4F24`, `4F2F` realizan giros -2,+2,+4. Los bits de peligrosidad/contacto no equivalen al hecho de tener comportamiento distinto de cero: `470D–478C` distingue jugador mediante flags internos y atributos del cuerpo antes de aplicar daño/desaparición.

## Trabajo pendiente

No se ha identificado todavía con evidencia suficiente la tabla de destinos de teletransporte ni su activación. No debe implementarse una asociación de teletransportes por cercanía o orden de habitación. Las frecuencias por tick y la distancia de movimiento de las familias anteriores dependen de507A/4FEF y necesitan seguir la reconstrucción antes de declarar equivalencia completa.

## Contacto: atributos originales frente a atributos en memoria

Los flags del descriptor no siempre son los flags finales del cuerpo. `372E–3749` detecta `(descriptorFlags&60)==60`, activa bit7 de runtime(IY+9) y borra bit6 de descriptorFlags; también borra bits2 y7 antes de guardar el atributo que corresponderá al cuerpo(IY+4). En consecuencia raw60 y raw6C se convierten en20 y28: peligrosos, no colectables. Los pickups creados por3F25 reciben60 directamente y no pasan por esa conversión.

`470D–471F` identifica jugador por runtime(IY+9)bit0 y ordena los dos participantes. `4725` exige bit5 del atributo del otro cuerpo. Si además tiene bit6, `4773–478C` lo transforma en desaparición y llama a3F5E: es un colectable. Si bit6 es cero, `4738–4770` aplica daño. Si argumentoA!=0 y el otro cuerpo tiene runtime9bit4,4730–4737 suprime daño en ese tipo de contacto.

La máscara de personajes dañados es3 si atributo jugador bit3 está activo (unidos); si no,2 cuando runtime9bit2 está activo, o1 en otro caso. Los flags runtime9 iniciales son21 y25. Un contador de protección247E positivo quita bit0 de la máscara;247F positivo quita bit1. Si la máscara queda cero no hay daño. Se acumula465A y se inicia temporizador4659=12 cuando procede. La deducción de vidas sucede al resolver la animación,26A8, no en cada tick de contacto.

La bolsa `2E02–2E30` requiere atributo de candidato bit6, igualdad de su Z a fondo jugador+6 o+7 unidades originales y solapamientoXY mediante2EC3. No requiere elegir objetos por sprite. Retirar mediante44CA llama3AF1 para desvincular la representación y marca atributo bit7, conservando el registro para poder dejarlo después.

## Identificación y consumo de mejoras

247C es recurso0 de velocidad. `29D0–29EB` permite un segundo paso cuando el personaje tiene activo&1 o recurso0>0; solo consume el recurso0 si activo carece de bit0, cada dos pasos mediante temporizador2489.

247D es recurso1 de salto. `2A53–2A69` permite altura adicional si activo&2; de otro modo consume recurso1 para obtener el mismo incremento. Esto identifica respectivamente las clases3 y4 como mejoras de velocidad y salto, sin depender del dibujo42 compartido.

247E y247F son recursos2/3 de inmunidad de los dos personajes. `257B–2595` los consume cada tres ticks (temporizador2488), en función de los bits del personaje activo; unidos consume ambos. La cancelación del daño se demuestra directamente en4745–4751. No son contadores de puntos de vida.
