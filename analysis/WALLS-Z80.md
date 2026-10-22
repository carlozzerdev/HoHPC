# Proyección y paredes originales

Confirmación estática en1E77: C=(X−Y+128)&255 y B=(2*Z−X−Y+127)&255. Para X=8*x,Y=8*y,Z=192−6*h, desplazamiento proyectado lógico es (8*(x−y),−8*(x+y)−12*h). Si se representa el aspecto original con píxel doble de ancho, queda (16*(x−y),−8*(x+y)−12*h). El origen vertical debe resolver el wrap de8bits explícitamente.

`wall-patterns.json` exporta bytes exactos de relocated.bin; no contiene capturas ni gráficos redibujados. Tabla de punteros3D9A:70F0,7390,7630,77F0,79B0,7D30,7EF0,8190. Tabla de selectores3DA9,8bytes por tema, consumida como campos MSB de2bits por3D7A/22B2. Estas tablas se solapan deliberadamente: el último byte alto del puntero es también primer selector.

1AC1 calcula dirección de variante como puntero+(selector&3)*224. En1A80, cada grupo4bytes(a,b,c,d) se distribuye a buffers: plano0(a,d), plano1(b,c). Son56filas de16píxeles, dos planos de2bytes; aspecto doble de ancho daría32×56. 1A9A refleja los4bytes por fila. Falta confirmar qué plano es imagen y cuál máscara en el blitter; no interpretar el registro como bitmap32×56 plano.

3C63/3C85 genera paredes según flags1EC0.bits2/3. 3D82 calcula origen y límites proyectados;3D10 guarda orientación80/00.3D03 lee selectores iniciales,3D0B–3D23 emite segmentos de pared y coordenadas en6A00;3D41–3D71 trata variantes de puerta4. Cada segmento se separa8unidades C y8unidades de extensión vertical según recorrido.

Precaución: las cuatro variantes exportadas por tema no implican que las cuatro sean válidas; tema7 variante3 alcanza8270, donde comienza otro banco de sprites. Completar selección geométrica antes de integrar todas indiscriminadamente.
