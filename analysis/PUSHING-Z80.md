# Empuje de objetos

Evidencia: disco facilitado `Head over Heels (Europe).dsk`, imagen de memoria
recuperada `relocated.bin` y desensamblado `original/z80.asm`.
`python tools/analyze_push.py` reconstruye la memoria desde el DSK y comprueba
271 instrucciones de los tramos relevantes byte a byte contra el desensamblado.
`push-evidence.json` conserva hashes, instrucciones y descriptores. Es evidencia
estática; no se ha realizado una comparación de ejecución con el CPC en este cambio.

## Regla observada

- `2998–29C3`: el jugador consulta su máscara de contacto, comprueba la dirección
  con `465C` y avanza mediante `4455` cuando se permite.
- `465C–470C`: la colisión encuentra el otro cuerpo. `46E5–46F9` aplica la máscara
  direccional al estado `+12` del cuerpo encontrado (o a su cuerpo principal si
  es compuesto). El contacto comunica presión, no convierte todos los sólidos en
  objetos móviles.
- `4DDC–4DE4`: gravedad `5029`, dirección solicitada `FF`, desplazamiento `4FEF`
  y actualización común. `4FEF–501E` toma la máscara de contacto, la traduce con
  `440F`, comprueba colisión y mueve un paso con `4452/4455`.
- `4455–446D`: incrementos X/Y de una unidad original, equivalentes a 1/8 de
  casilla nativa. `4FC7` vuelve a dejar la máscara de contacto en `FF`.
- El muelle (tipo **1**, descriptor `2B 1C 40`, conducta **28**) entra en `4D98`:
  maneja sprites 43/44 y temporizador 80, y llega a la misma rutina `4DDC`.
- La conducta **10**, `4C86–4CA6`, también llega a `4DDC` en reposo. Sus ramas de
  movimiento previo utilizan `4CA7`; la inercia completa sigue pendiente.
- La bolsa comprueba el bit `40` en `2E18`. Ese permiso es independiente del
  comportamiento que admite desplazamiento: hay objetos empujables sin bolsa.

## Inventario de cuerpos pasivos que responden al empuje

| Tipos originales | Conducta | Instancias | Bolsa |
|---|---|---:|---|
| 1 (muelle) | 28 | 22 | Sí |
| 3, 10, 11 | 1 | 72 | Sí |
| 21, 40, 52 | 1 | 22 | No |
| 13, 48, 51 | 10 | 43 | No |
| 29 | 10 | 13 | No; mantiene daño por contacto |

Total: **11 tipos, 172 instancias** recuperadas. La clasificación usa la conducta
y los atributos originales, nunca el aspecto del dibujo. Conducta 0 permanece
estática; 2–5 inicializan máscaras direccionales y pasan a 0. Interruptores,
plataformas verticales, enemigos y cuerpos que desaparecen conservan sus reglas
separadas; no se les aplica el empuje genérico de cuerpos pasivos.

## Implementación y alcance

`Pushing.cs` resuelve contacto lateral, volúmenes originales, empujes diagonales,
cadenas y límites de habitación. Comprueba todos los cuerpos antes de confirmar
un desplazamiento, de modo que una cadena bloqueada no mueve parcialmente sus
elementos. Un cuerpo admite un paso por tick aunque Heels intente caminar dos
veces. Los objetos congelados no responden. El otro personaje bloquea el empuje.
La posición pertenece al estado de la sala; los datos recuperados son inmutables.

Se activa la gravedad existente también para las conductas 10 y 28, con soporte
en otros cuerpos y personajes. Caminar por encima no empuja; al retirar el apoyo
un cuerpo móvil puede caer. Empujar no anula el daño ni concede permiso de bolsa.

La resolución de cadenas es una adaptación nativa atómica en el mismo tick; el
original propaga máscaras entre actualizaciones. No se certifica identidad de
latencias, inercia de conducta 10, compresión temporizada del muelle ni todas las
interacciones con enemigos, cintas y cuerpos compuestos. El salto aumentado y
la compresión del muelle se añadieron en la revisión posterior descrita en
`RECUPERAR-MECANICAS.md`.

`PushVerification.cs` comprueba ambos personajes, las cuatro direcciones y diagonal,
los 11 descriptores reales, límites, cadenas bloqueadas, alturas, soporte, daño,
congelación, persistencia y frecuencia de simulación. `--verify` también desplaza
el muelle de la sala original **0480** de X=4 a X=5 en ocho ticks y guarda
`push-spring-before.png` y `push-spring-after.png`.
