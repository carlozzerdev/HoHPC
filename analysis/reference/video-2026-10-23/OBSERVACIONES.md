# Referencia de juego aportada el 23 de octubre de 2026

Fuente: `Grabación de pantalla 2026-10-23 183733.mp4`, en la raíz del proyecto.
Duración: 172,933 s; 852 × 508, 30 fps; audio AAC estéreo a 48 kHz.
Los tiempos siguientes se cuentan desde el inicio del archivo.

## Material derivado

- `sheet-01.png`: muestras cada dos segundos, de 00:00 a 00:58.
- `sheet-02.png`: de 01:00 a 01:58.
- `sheet-03.png`: de 02:00 al final. Las celdas finales vacías no son imágenes del vídeo.
- `death-detail.png`: muestras cada 250 ms entre 01:52 y 02:02. Sus rótulos son relativos a 112 s; hay que sumar ese valor.
- `reference.wav`: audio completo, convertido a PCM mono de 22.050 Hz para análisis posterior. El MP4 conserva la pista original.

## Índice visual y contraste con el prototipo

| Tramo aproximado | Observación visual | Aplicación al proyecto |
| --- | --- | --- |
| 00:00–00:24 | Movimiento y saltos de Head en la sala inicial; plataforma de teletransporte al fondo. | Referencia para cadencia de pasos, altura y duración de salto. La grabación no incluye el estado de las teclas. |
| 00:24–00:42 | Ascenso y salto junto a la pared izquierda y sobre ella. | Revisar altura visible y recorte de cámara; no deducir todas las reglas de colisión de un fotograma. |
| 00:42–00:50 | Personaje sobre la plataforma y cambio a una sala de otra paleta. | Acotar salida, desaparición, llegada y aparición con fotogramas consecutivos antes de ajustar los contadores actuales de 12/8 ticks. |
| 00:54–01:02 | Sala con superficie negra y plataformas elevadas, seguida de otra con rejilla verde. | Distinguir ausencia de suelo, suelo peligroso y suelo normal: el color negro por sí solo no identifica la regla. |
| 01:04–01:20 | Pasillo estrecho con varias puertas y un bloque alargado rosa; visita intermedia a una sala morada. | Conservar proporciones propias y alturas de puertas; no equiparar todas las salas a un cuadrado. |
| 01:22–01:34 | Otro pasillo estrecho y una sala azul con dos franjas largas de objetos. | Referencia adicional de geometría y disposición. |
| 01:38–01:40 y 02:04–02:18 | Sala con muelle, columna y objetos móviles; varios saltos en la segunda visita. | Comparar compresión, liberación y recuperación del muelle y sincronizar su sonido con el evento visual. |
| 01:42–02:02 | Sala de columnas y pasarela con suelo rojo de patrón dentado/curvo, distinto de la rejilla. | Es la referencia señalada por el usuario para suelo que mata/pinchos. El renderizador actual dibuja una rejilla genérica para todos los suelos presentes. |
| 01:52–02:02 | Cambios de altura entre columnas, caída hacia la zona central y retorno a la pasarela junto a la puerta superior izquierda. | Candidato para estudiar muerte y reaparición. El muestreo visual no permite fijar todavía el primer frame de daño ni confirmar toda la secuencia. No convertir este caso en una regla universal de retorno. |
| 02:26–02:34 | Recorrido por sala azul de franjas largas. | Comprobar espacio jugable, obstáculos y orden de dibujo. |
| 02:36–02:44 | Sala ancha con varias filas elevadas y puertas a diferentes alturas. | Referencia de salas mayores, plataformas y profundidad. |
| 02:46–02:52 | Pasillo con plataformas rosas elevadas y suelo de otro patrón dentado. | Segunda referencia de suelo especial y geometría estrecha. |

## Diferencias comprobadas en el código actual

- `Renderer.DrawRoom` usa una rejilla provisional y solo distingue suelo presente/ausente mediante `HasFloor`. No representa visualmente las variantes peligrosas del vídeo.
- `Character.Sprite` solo selecciona ciclos normales de orientación y marcha. El renderizador no consulta `DamageTicks` ni el estado de teletransporte para elegir una animación específica.
- `NativeGame.Damage` espera 12 ticks y llama a `Respawn`. Sin checkpoint recogido, `Respawn` coloca X/Y en 3,5/3,5 de la sala actual. Es necesario contrastar el punto de retorno original; la posición de entrada observada es una hipótesis para este caso, no una regla ya recuperada.
- El suelo especial necesita contraste de datos y lógica: no basta con dibujar pinchos ni con marcar todas las superficies negras como letales.

## Sonido: estado de la comprobación

El usuario señala pasos, salto, muelle, teletransporte y muerte como referencias auditivas. Se conserva la pista y se ha extraído una copia de trabajo. Esta revisión ha inspeccionado imágenes; **no constituye una escucha ni una validación de fidelidad sonora**. Quedan pendientes los cortes precisos por evento, comparación de duración/espectro con la salida nativa y revisión de prioridades y solapamientos. Las asignaciones recuperadas del disco se documentan en `analysis/original-audio.md`; no sustituirlas por aproximaciones basadas únicamente en estos fotogramas.

## Pendiente antes de cambiar reglas

1. Identificar las salas del recorrido mediante sus objetos, límites y salidas recuperados.
2. Acotar muerte y teletransporte a resolución de frame y separar animación de muerte, pérdida de vida, espera y reaparición.
3. Recuperar selección de suelo y condición letal del original; contrastar ambas salas con patrón dentado.
4. Contrastar punto de retorno por personaje, entrada, teletransporte y checkpoint; el vídeo no cubre todos esos casos.
5. Comparar audio por evento y medir sincronización con las animaciones.

No se ha modificado la simulación ni se declara equivalencia con el original a partir de este registro.
