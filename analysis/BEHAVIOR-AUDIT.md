# Auditoría de comportamientos CPC

Actualización posterior: esta revisión ha añadido salto aumentado/animación de
muelle (28), plataformas verticales (16/18/23) y pasos de planta. También ha
corregido persecución 15/33 y orden PRNG 9/12/13. La tabla siguiente conserva el
diagnóstico de partida; el estado final y las pruebas están en
`RECUPERAR-MECANICAS.md` y `mechanics-evidence.json`. Estas correcciones siguen
sin certificar equivalencia de todas las máscaras y tiempos del original.

Auditoría estática de la versión previa a las correcciones de muelle y ascensor de esta sesión. Fuente primaria: `original/z80.asm`, contrastable con direcciones de `relocated.bin`. Las direcciones son de memoria Z80. «Parcial» significa que existe código nativo, no equivalencia temporal o física demostrada. No se ha realizado validación dinámica contra el original en esta auditoría.

## Inventario de despacho

`3764–378B` despacha mediante `387F+2*(id-1)`. Los estados de contacto IY+11/IY+12 y auxiliar IY+17 son parte de la mecánica, no metadatos opcionales.

| ID | Dirección | Estado antes de esta sesión | Diferencia concreta |
|---|---|---|---|
|0|sin despacho|Parcial|Geometría estática recuperada; no certifica interacción de cuerpos.|
|1|4DDC|Parcial|Gravedad/empuje aproximados; faltan máscaras de contacto.|
|2|4D46|Parcial|Se convierte a0, pero falta instalar máscara FE.|
|3|4D4A|Parcial|Se convierte a0, falta máscara FD.|
|4|4D4E|Parcial|Se convierte a0, falta máscara F7.|
|5|4D52|Parcial|Se convierte a0, falta máscara FB.|
|6|4DF3|Parcial|Rebote +4 correcto; condiciones de contacto simplificadas.|
|7|4DF8|Parcial|Giro -2 correcto; condiciones simplificadas.|
|8|4DFD|Parcial|Giro +2 correcto; condiciones simplificadas.|
|9|4E11|Parcial|Aleatorio impar y probabilidad1/16 presentes; orden gravedad/PRNG distinto.|
|10|4C86|Parcial|Empuje/gravedad; faltan interacción4CA7 y reinicio de estado4CA2.|
|11|4E02|Parcial|Aleatorio8 al contacto, condiciones simplificadas.|
|12|4E07|Parcial|Aleatorio8 periódico, orden y contacto simplificados.|
|13|4E0C|Parcial|Aleatorio par periódico, orden y contacto simplificados.|
|14|4CD6|Pendiente|Interruptor y alternancia de flags de otros cuerpos ausentes.|
|15|4E16|Parcial|Elige eje dominante;4F55 elige el menor eje no nulo.|
|16|4E82|Pendiente|Movimiento vertical y transporte de soporte ausentes.|
|17|4D80|Pendiente|Desaparición por animación ausente.|
|18|4D31|Pendiente|Inicialización52 y transición16 ausentes.|
|19|4D3B|Pendiente|Gravedad condicional bit5 ausente.|
|20|4D66|Pendiente|Desaparición por contacto/máscaras ausente.|
|21|4DED|Parcial|Giro -2 sin gravedad inicial presente; resto simplificado.|
|22|4DE7|Parcial|Rebote+4 sin gravedad inicial presente; resto simplificado.|
|23|4D2E|Pendiente|Inicialización90 y transición16 ausentes.|
|24|4D63|Pendiente|Desaparición condicional C0 ausente.|
|25|4C76|Pendiente como entidad|Existe ruta de proyectil separada, no este despacho de entidad.|
|26|4DCF|Parcial por ruta separada|Los pickups se simulan aparte; no equivalencia de animación y contacto demostrada.|
|27|4C18|Pendiente|Actualización condicionada bit5 ausente.|
|28|4D98|Parcial|Se empuja y cae; faltan compresión43/44, temporizador80 y rebote.|
|29|4BFB|Pendiente|Consumo de dirección compartida4BEA ausente.|
|30|4BEB|Pendiente|Publicación de dirección compartida4BEA ausente.|
|31|4D5C|Pendiente|Desaparición condicionada activo&2 ausente.|
|32|4C5E|Pendiente|Interacción por dirección relativa ausente.|
|33|4F36|Parcial|Umbral3 correcto; se pierde persecución diagonal al agrupar con15.|
|34|4D92|Pendiente|Retirada explícita44CA ausente.|
|35|4C3F|Pendiente|Contexto temporal4C3E e interacción4C4C ausentes.|
|36|4C29|Pendiente|Temporizador compartido96 y transición25 ausentes.|
|37|4E1B|Parcial|Persigue/huye según cuatro coronas correcto; contacto y temporización simplificados.|

Ninguna familia móvil debe etiquetarse «completa» sin contraste por ticks. Recuperar los 301 escenarios y sus sprites no recupera automáticamente su lógica ejecutable.

## Correcciones portables demostradas

### Persecución15 frente a33

`4F79` calcula D=abs(dy), E=abs(dx), y C como máscara de dirección relativa. `440F` traduce la máscara mediante tabla441A. `4F55` usa F3 si D<E (conserva dirección Y), FC en caso contrario (conserva dirección X). Si la distancia del eje elegido es cero, XOR0F cambia el eje conservado. Por tanto15 selecciona el eje menor no nulo, con empate en X. Pseudocódigo equivalente en coordenadas nativas:

```csharp
int dx=Math.Sign(player.X-e.X),dy=Math.Sign(player.Y-e.Y);
bool useY=Math.Abs(player.Y-e.Y)<Math.Abs(player.X-e.X);
if((useY?dy:dx)==0)useY=!useY;
if(useY)dx=0;else dy=0;
```

33 llama directamente a4F79 y440F, conservando ambos ejes. Solo activa desplazamiento si ambas distancias son <=24 unidades originales, es decir3 nativas (`4F3C–4F4C`). Fuera del radio aún actualiza animación mediante4FDF; no significa congelar la entidad.

37 conserva dirección diagonal; `4E1B–4E29` selecciona4F6D (XOR0F, huida) si los cuatro bits inferiores de3DEA son1, y4F74 (persecución) en otro caso. La quinta corona no altera esta bifurcación.

### Enemigos6–13,21,22

Los operadores finales son correctos:4F1D resta2,4F24 suma2,4F2F suma4, todo módulo8.4EF4 restringe pares,4F01 impares,4F10 ocho direcciones y repite si coincide con anterior.

Sin embargo4E31 no equivale a «mover, si falla girar»:4FDF actualiza orientación/animación;5029 resuelve gravedad;507A obtiene máscara direccional de5086;4FEF intersecta con IY+12, convierte440F y decide movimiento/resultado4BE9. Solo después decide si girar. Un único booleano de colisión omite contacto direccional y desplazamiento secundario. Las variantes21/22 entran por4E2B y omiten5029 inicial, detalle ya representado parcialmente.

En9/12/13 el PRNG se consulta ANTES de la gravedad (`4E65–4E7E`). El código previo descartaba el tick al caer antes de consultar PRNG: cambia toda la secuencia futura incluso usando el generador correcto. Conviene preservar el orden de llamadas del original antes de comparar trayectorias.

La dirección inicial necesita seguir la asignación del registro entero y su reconstrucción.36F5 inicializa flags y conducta pero no escribe IY+16. No se puede justificar una dirección por sprite ni afirmar que todas empiecen en0 solo porque el campo C# tiene valor predeterminado0. Se debe capturar IY+16 tras construcción de sala en el original y localizar la copia/limpieza que lo establece.

### Interacciones32/35

32:4C5E consulta36F0 (contacto anterior); si existe interacción4C92. Si no,4F79 calcula dirección relativa, compara máscaras F3/FC y escribe IY+12 en4C71 antes de4C92. No es otro perseguidor genérico.

35:4C3F pone4C3E=FF, llama4C4C y restaura0.4C4C usa contacto previo y bit5 deIY+12; puede obtener BC de24AB. Requiere conservar el contexto temporal y la ruta de interacción común. No sustituirlo por daño por proximidad.

## Cómo obtener lo que falta del original

1. Mantener el DSK y la imagen relocalizada como fuentes reproducibles (`tools/recover.py`). Las tablas dan descriptor, rutina y cantidades; no incluyen una descripción en texto de cada mecánica.
2. Para cada ID pendiente, seguir la rutina de despacho y todas sus dependencias. Conservar campos de estado y máscaras; registrar unidades originales frente a nativas (XY/8, altura/6).
3. Usar el emulador original como referencia externa: mismo snapshot y entradas por tick; capturar sala, objeto, XYZ, dirección, auxiliar, contacto y sprite antes/después de cada tick. El ejecutable nativo final puede seguir sin emulador.
4. Comparar escenarios mínimos: caída libre; choque de pared por ocho direcciones; contacto entre objetos; jugador encima; jugador lateral; cambio de sala; congelación. Una prueba nativa que solo confirma su propia implementación no demuestra fidelidad.
5. Para audio, registrar escrituras AY-3-8912 y tiempos desde el original, o traducir el secuenciador y tablas que las producen. Un sonido sintetizado elegido por parecido no es extracción del original.

La causa verificable de las ausencias es un port incompleto: el filtro de StepObjects deja sin ejecutar la mayoría de IDs, y la documentación PORT-STATUS ya los enumera como pendientes. Las afirmaciones anteriores de comprobaciones no certificaban equivalencia global.
