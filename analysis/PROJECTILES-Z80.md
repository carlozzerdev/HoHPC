# Proyectil y generador aleatorio originales

## Creación y movimiento

`25D5–2639` permite disparar solo con activo==2, inventario247B bits1 y2, acción24AC cero y estado24A8==8. Copia XYZ desde24C7–24C9 a249E–24A0 del cuerpo plantilla2499. Su sprite inicial es24 hexadecimal (36 decimal), no19:19 hexadecimal es el comportamiento25 escrito en24A3. El cuerpo tiene atributo4=0, runtime9=08, máscara de dirección11 copiada24AB, máscara12=FF, estado de animación15=20 hexadecimal(32). Consume una unidad del recurso6 (munición2482); al agotarlo borra inventario bit2. El parámetro32 es estado de animación, no una vida útil demostrada de32 ticks.

Conducta25 despacha a4C76. Primero actualiza animación4FE2, después llama4CA7 y, si no hubo choque, vuelve a llamar4CA7. Cada llamada puede realizar dos pasos4452, por tanto el proyectil realiza hasta cuatro pasos por tick. Un choque pasa a4D6D: conducta17, animación08, atributo80; completa desaparición y retira el cuerpo.

`4CA7` combina máscara de dirección previa36F0 con máscara de contactosIY+12, convierte a sector con440F y comprueba colisión mediante4CD0/465C. Si hay espacio, prepara repintado4FB6 y ejecuta4452. Luego4CC2 comprueba el byte4C3E; si esFF retorna y si no realiza otra comprobación4CD0 y otro paso4452 en4CCB. La única escritura localizada de4C3E es el wrapper de conducta35 en4C3F: poneFF, llama4C4C y restaura00 antes de retornar4C4B. Los despachos de conducta son secuenciales; durante4C76 el byte es00. No se debe aplicar al proyectil la limitación de un solo paso por4CA7 de conducta35.

`4452–446E` usa tabla442A para sumar un desplazamiento entero a X y Y, actualiza los cuatro bits inferiores de máscara11 y conserva Z.

| Sector | dX original | dY original | Máscara baja |
|---|---|---|---|
| 0 | -1 | 0 | 13 |
| 1 | -1 | -1 | 9 |
| 2 | 0 | -1 | 11 |
| 3 | 1 | -1 | 10 |
| 4 | 1 | 0 | 14 |
| 5 | 1 | 1 | 6 |
| 6 | 0 | 1 | 7 |
| 7 | -1 | 1 | 5 |

Cada paso equivale a1/8 de unidad de rejilla por eje. La velocidad diagonal no está normalizada. La frecuencia del tick debe reproducir la cadencia original para mantener la velocidad observada.

## Impacto: congelación, no eliminación

`478F–47B6` detecta un participante con runtime9bit3 (el proyectil), selecciona el otro cuerpo enIX y, si es la segunda mitad de entidad compuesta(bit1), retrocede18 bytes. Exige runtime9bit7 en el objetivo. Si está activo, marca runtime9bit6 y dirección11=FF.

`3786–378A` retorna inmediatamente cuando runtime9bit6 está activo. El enemigo queda congelado sin cambiar su posición ni eliminar el registro. Los descriptores con `(flags&60)==60` reciben runtime9bit7 en3735, por tanto son los objetivos demostrados. No se ha encontrado aquí un temporizador automático que quite la congelación; no introducir una duración inventada. Reconstruir habitación restablece naturalmente los flags desde los descriptores, aunque persistencia exacta requiere seguimiento del ciclo completo de entradas.

## PRNG4497–44C5

Portado en `src/OriginalRandom.cs`. Semillas almacenadas en44C6=6F4A y44C8=216E. Conserva acarreo entre ADD/ADC/RL, bit de signo de ADC antes del RES7,H y propagación de acarreo de la secuenciaINC. El retornoHL es la primera palabra.

Vectores derivados con una traducción independiente orientada a registros, ejecutando cada instrucción del listado por separado. Tras cada llamada las dos palabras deben ser:

| Llamada |44C6/retorno|44C8|
|---|---|---|
|1|4DCF|7E4D|
|2|E9C9|18E7|
|3|BC2C|68BB|
|4|35CE|6035|
|5|A14E|4A9F|
|6|E478|09E2|
|7|AC9F|4FAC|
|8|05DD|2906|
|9|1082|3510|
|10|3157|5530|

El puerto no emula CPU ni hardware: es una función C# de actualización de dos palabras. No se cambian probabilidades ni se sustituye por Random de .NET.
