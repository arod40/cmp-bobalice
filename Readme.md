# BobAlice

Este repositorio contiene un evaluador **Bob** (`bob.py`) y varios ejemplos de prueba.
El archivo `random_alice.py` contiene una solución trivial que sirve de base para
probar tu eficiencia (al menos tienes que ser mejor que el aleatorio), y como
base para tu código. Para ejecutarlo es necesario pasarle a **Bob** el comando para
ejecutar a **Alice**.

    python3 bob.py python3 random_alice.py

El archivo `greedy_alice.py` tiene una implementación un tilín más compleja que
simplemente busca la cadena más cercana.

    python3 bob.py python3 random_alice.py

Para probar tu código, supongamos que está implementado en Python 3 (verdad?), y
se llama `super_awesome_alice.py`, tienes que ejecutar lo siguiente:

    python3 bob.py python3 super_awesome_alice.py

Si tu **Alice** está en otro lenguaje, pero también funciona por la entrada y
salida estándar, simplemente le tienes que pasar a **Bob** el comando correcto.

En .NET:

    python3 bob.py SolidAlice.exe

En Ruby:

    python3 bob.py ruby shiny_alice.rb
