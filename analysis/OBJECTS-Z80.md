# Entidades y geometría recuperadas del Z80

Análisis estático de `analysis/original/z80.asm` contrastado con los bytes de `analysis/relocated.bin`. No ejecuta ni emula el Z80.

## Descriptores

`207E–2091` lee el tipo de objeto y llama a `36F5`. Los descriptores de tres bytes comienzan en `38C9`: sprite/código de animación, comportamiento y bits auxiliares, flags de forma. `3706–3709` conserva los seis bits inferiores como comportamiento. `376B–378B` despacha a la dirección de la tabla `387F + 2*(comportamiento-1)`. Cero es estático. Exportamos `descriptor` y `behaviorAddress` sin atribuir nombres especulativos a estas rutinas.

Los dos bits superiores del segundo byte seleccionan un sprite auxiliar en `36F2–36F4`, valores `3D,8E,3D`. `3720–3726` intercambia principal y auxiliar cuando está activo el bit 2 de flags. Se exportan `secondarySprite` y `secondaryAnimation`. No se duplican los cuerpos físicos. La representación compuesta y su colocación necesitan aún comparación visual.

## Animaciones

`374E` inicializa el estado con `(sprite*8)&255`; `37AB–37DE` avanza su contador y lee una secuencia terminada en cero. La tabla de punteros comienza efectivamente en `37F3` para el código `81`; la fórmula equivalente es `37F1+2*(sprite&31)`. Índice cero es especial y retorna antes de consultar el puntero: `37F1` contiene instrucciones SCF/RET, no un puntero válido. El recuperador anterior trataba ese caso como puntero y usaba siempre frame cero. Ahora conserva secuencias completas en `animation` y exporta el frame inicial uno, con vuelta al cero para secuencias de un solo elemento.

## Cajas de colisión

`226E–229C` convierte la rejilla a centros originales X/Y `12+8*coordenada` y Z `150+6*coordenada`. El motor nativo representa altura hacia arriba como `7-z`. `3B2F–3BA4` calcula las siguientes cajas, independientemente de las dimensiones del dibujo:

| Flags bajos | Intervalo X original relativo | Intervalo Y | Altura |
|---|---|---|---|
| 0 | -3 a +3 | -3 a +3 | 6 |
| 1 | -4 a +4 | -4 a +4 | 6 |
| 2 | -4 a +4 | -1 a +1 | 6 |
| 3 | -1 a +1 | -4 a +4 | 6 |
| 4–7 | 0 a +4, o -4 a 0 según bit 0 | 0 a +4, o -4 a 0 según bit 1 | 18 |

`collider` exporta intervalos X/Y divididos entre ocho y altura dividida entre seis. Son extents relativos al centro recuperado, no límites absolutos de habitación. `3B24` aplica además un ajuste vertical de seis unidades para flags bit 3; su uso concreto por cada regla sigue pendiente. Bit 3 también provoca entidades gráficas compuestas en `3A0A–3A38`; no debe confundirse automáticamente con peligrosidad o posibilidad de recoger.

## Puertas y límites de fidelidad

`2121–214E` distingue códigos de salida 0, 1 y 2–7. Solamente 2–7 construyen marcos visibles. Su altura original es `150+6*(9-código)`, equivalente a altura nativa `código-2`. No basta comprobar `exit!=0` para traducir toda la semántica de puertas.

Este análisis no identifica todavía con suficiente evidencia todos los pickups, vidas, objetivos o enemigos. Se conserva la dirección original de comportamiento para poder reconstruirlos sin inferir funciones por el aspecto de un sprite.

## Comprobación de dimensiones de sprites

`363F–3656` calcula `8BD0+144*(sprite-24)` para sprites24–83. La máscara empieza72 bytes después y el bucle de transformación recorre48 grupos de3 bytes (ambos planos). Cada plano contiene24 filas de3 bytes: ancho24 píxeles y alto24,144 bytes totales. La extracción `w=3,h=12`, seguida de24 filas y dos planos72, es coherente con esas instrucciones.

`367C–3692` calcula `AA30+224*(sprite-84)` con máscara a112 bytes y56 grupos de4 bytes para ambos planos. Son28 filas de4 bytes por plano:32x28 píxeles. `3612–3625` confirma el stride192 y máscara96 de sprites16–23:24x32 píxeles. No hay motivo estático para duplicar de nuevo la altura ni interpretar144 como tamaño de cada plano.
