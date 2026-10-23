# Habitaciones continuas

4260 contiene la primera fila de seis objetos;4360 contiene la segunda.
Ambos registros tienen shape7, l?mites[16,16,64,64] y salida1 rec?proca
(lado0 de4260 y lado2 de4360). No son dos habitaciones cerradas.

Evidencia Z80 en analysis/original/z80.asm, contrastada con relocated.bin:
2121?214E distingue c?digo0, c?digo1 y puertas2?7 al construir flags1EC0.
1F55?1F77 carga el vecino Y cuando falta la pared, y1F7F?1F9F hace lo
mismo en X.1FB8?2008 calcula el desplazamiento desde el l?mite del primer
registro menos el l?mite inicial del segundo. Para4260?4360,64?16=48
unidades originales, equivalentes a6 casillas: desplazamiento(0,6),
proyectado(-96,-48). As? coinciden los bordes de ambos suelos.

RoomConnections agrupa enlaces1 rec?procos y calcula estas posiciones.
Renderer omite la pared de la uni?n y compone ambas partes en las vistas.
GameWindow descubre el grupo completo al entrar y centra su c?mara en el
conjunto. NativeGame permite el paso por todo el borde y conserva altura,
ascenso y estado a?reo al cruzarlo; las puertas2?7 mantienen sus reglas.

ConnectedRoomVerification comprueba110 condiciones, incluidas las uniones
recuperadas y el recorrido4260?4360. VerifyContinuousRoom comprueba que la
segunda mitad est? visible y que cambiar de registro no mueve la c?mara.
Las capturas generadas se han revisado visualmente. No se han convertido
los registros en un estado ?nico de simulaci?n ni recuperado todas las
interacciones de entidades a trav?s de las uniones.
