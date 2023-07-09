
using System;
using System.Collections.Generic;
using System.IO;

class Program
{
    static Dictionary<string, string> usuarios = new Dictionary<string, string>(); // Usuarios registrados (usuario, contraseña) 
    static Dictionary<string, string> textos = new Dictionary<string, string>(); // Textos registrados (nombre, contenido)
    static List<HistorialBusqueda> historialBusqueda = new List<HistorialBusqueda>(); // Historial de búsqueda

    static void Main()
    {
        // Registrar usuarios
        RegistrarUsuario("usuario1", "contrasena1");
        RegistrarUsuario("usuario2", "contrasena2");

        // Menú principal
        bool sesionIniciada = false;
        string usuarioActual = "";
        while (!sesionIniciada)
        {
            Console.Write("Ingrese el usuario: ");
            string usuario = Console.ReadLine();
            Console.Write("Ingrese la contraseña: ");
            string contraseña = Console.ReadLine();

            if (AutenticarUsuario(usuario, contraseña))
            {
                sesionIniciada = true;
                usuarioActual = usuario;
                Console.WriteLine("Inicio de sesión exitoso.\n");
            }
            else
            {
                Console.WriteLine("Credenciales incorrectas. Intente nuevamente.\n");
            }
        }

        bool salirPrograma = false;
        while (!salirPrograma)
        {
            Console.WriteLine("===== MENÚ PRINCIPAL =====");
            Console.WriteLine("1. Registrar un texto");
            Console.WriteLine("2. Buscar palabra / oración en un texto");
            Console.WriteLine("3. Ver historial de búsquedas");
            Console.WriteLine("4. Salir");

            Console.Write("Seleccione una opción: ");
            string opcion = Console.ReadLine();
            Console.WriteLine();

            switch (opcion)
            {
                case "1":
                    RegistrarTexto();
                    break;
                case "2":
                    BuscarTexto(usuarioActual);
                    break;
                case "3":
                    MostrarHistorialBusqueda();
                    break;
                case "4":
                    salirPrograma = true;
                    Console.WriteLine("Saliendo del programa...");
                    break;
                default:
                    Console.WriteLine("Opción inválida. Intente nuevamente.\n");
                    break;
            }
        }
    }

    static void RegistrarUsuario(string usuario, string contraseña)
    {
        usuarios.Add(usuario, contraseña);
    }

    static bool AutenticarUsuario(string usuario, string contraseña)
    {
        if (usuarios.ContainsKey(usuario) && usuarios[usuario] == contraseña)
        {
            return true;
        }
        return false;
    }

    static void RegistrarTexto()
    {
        Console.Write("Ingrese el nombre del texto: ");
        string nombreTexto = Console.ReadLine();
        Console.Write("Ingrese la ruta del archivo de texto: ");
        string rutaArchivo = Console.ReadLine();

        if (!File.Exists(rutaArchivo))
        {
            Console.WriteLine("El archivo no existe. No se pudo registrar el texto.\n");
            return;
        }

        string contenidoTexto = File.ReadAllText(rutaArchivo);
        textos.Add(nombreTexto, contenidoTexto);
        Console.WriteLine("Texto registrado exitosamente.\n");
    }

    static void BuscarTexto(string usuarioActual)
    {
        Console.WriteLine("===== BUSCAR PALABRA / ORACIÓN EN UN TEXTO =====");
        Console.WriteLine("Textos registrados:");

        foreach (var texto in textos)
        {
            Console.WriteLine("- " + texto.Key);
        }

        Console.Write("Seleccione el nombre del texto: ");
        string textoSeleccionado = Console.ReadLine();

        if (!textos.ContainsKey(textoSeleccionado))
        {
            Console.WriteLine("El texto seleccionado no existe.\n");
            return;
        }

        Console.WriteLine("Seleccione el algoritmo de búsqueda:");
        Console.WriteLine("1. Fuerza Bruta");
        Console.WriteLine("2. Knuth-Morris-Pratt");
        Console.WriteLine("3. Boyer-Moore");
        Console.Write("Seleccione una opción: ");
        string algoritmoSeleccionado = Console.ReadLine();
        Console.WriteLine();

        Console.Write("Ingrese la palabra / oración a buscar: ");
        string consultaBusqueda = Console.ReadLine();
        Console.WriteLine();

        string contenidoTexto = textos[textoSeleccionado];

        DateTime horaInicio = DateTime.Now;

        List<int> ocurrencias;
        switch (algoritmoSeleccionado)
        {
            case "1":
                ocurrencias = BuscarFuerzaBruta(contenidoTexto, consultaBusqueda);
                break;
            case "2":
                ocurrencias = BuscarKnuthMorrisPratt(contenidoTexto, consultaBusqueda);
                break;
            case "3":
                ocurrencias = BuscarBoyerMoore(contenidoTexto, consultaBusqueda);
                break;
            default:
                Console.WriteLine("Opción inválida. Se utilizará Fuerza Bruta por defecto.\n");
                ocurrencias = BuscarFuerzaBruta(contenidoTexto, consultaBusqueda);
                break;
        }

        DateTime horaFin = DateTime.Now;
        TimeSpan tiempoBusqueda = horaFin - horaInicio;
        int cantidadOcurrencias = ocurrencias.Count;

        // Mostrar resultados
        Console.WriteLine($"Resultado de la búsqueda en el texto '{textoSeleccionado}':");
        Console.WriteLine($"Palabra / oración buscada: {consultaBusqueda}");
        Console.WriteLine($"Tiempo de duración de la búsqueda: {tiempoBusqueda.TotalMilliseconds} ms");
        Console.WriteLine($"Cantidad de apariciones: {cantidadOcurrencias}\n");

        // Actualizar historial de búsqueda
        historialBusqueda.Add(new HistorialBusqueda(usuarioActual, textoSeleccionado, consultaBusqueda, tiempoBusqueda, cantidadOcurrencias));

        // Ordenar historial de búsqueda por cantidad de apariciones (de mayor a menor)
        historialBusqueda.Sort((x, y) => y.CantidadOcurrencias.CompareTo(x.CantidadOcurrencias));
    }

    static List<int> BuscarFuerzaBruta(string texto, string patron)
    {
        List<int> ocurrencias = new List<int>();
        int n = texto.Length;
        int m = patron.Length;

        for (int i = 0; i <= n - m; i++)
        {
            int j;
            for (j = 0; j < m; j++)
            {
                if (texto[i + j] != patron[j])
                {
                    break;
                }
            }

            if (j == m) // Coincidencia encontrada
            {
                ocurrencias.Add(i);
            }
        }

        return ocurrencias;
    }

    static List<int> BuscarKnuthMorrisPratt(string texto, string patron)
    {
        List<int> ocurrencias = new List<int>();
        int n = texto.Length;
        int m = patron.Length;

        int[] lps = new int[m];
        int j = 0;

        ComputeLPSArray(patron, m, lps);

        int i = 0;
        while (i < n)
        {
            if (patron[j] == texto[i])
            {
                j++;
                i++;
                if (j == m)
                {
                    ocurrencias.Add(i - j);
                    j = lps[j - 1];
                }
            }
            else if (i < n && patron[j] != texto[i])
            {
                if (j != 0)
                {
                    j = lps[j - 1];
                }
                else
                {
                    i++;
                }
            }
        }

        return ocurrencias;
    }

    static void ComputeLPSArray(string patron, int m, int[] lps)
    {
        int len = 0;
        int i = 1;
        lps[0] = 0;

        while (i < m)
        {
            if (patron[i] == patron[len])
            {
                len++;
                lps[i] = len;
                i++;
            }
            else
            {
                if (len != 0)
                {
                    len = lps[len - 1];
                }
                else
                {
                    lps[i] = 0;
                    i++;
                }
            }
        }
    }

    static List<int> BuscarBoyerMoore(string texto, string patron)
    {
        List<int> ocurrencias = new List<int>();
        int n = texto.Length;
        int m = patron.Length;

        int[] badChar = new int[256];

        BadCharHeuristic(patron, m, badChar);

        int s = 0;
        while (s <= (n - m))
        {
            int j = m - 1;

            while (j >= 0 && patron[j] == texto[s + j])
            {
                j--;
            }

            if (j < 0)
            {
                ocurrencias.Add(s);
                s += (s + m < n) ? m - badChar[texto[s + m]] : 1;
            }
            else
            {
                s += Math.Max(1, j - badChar[texto[s + j]]);
            }
        }

        return ocurrencias;
    }

    static void BadCharHeuristic(string patron, int m, int[] badChar)
    {
        for (int i = 0; i < 256; i++)
        {
            badChar[i] = -1;
        }

        for (int i = 0; i < m; i++)
        {
            badChar[(int)patron[i]] = i;
        }
    }

    static void MostrarHistorialBusqueda()
    {
        Console.WriteLine("===== HISTORIAL DE BÚSQUEDAS =====");

        foreach (var busqueda in historialBusqueda)
        {
            Console.WriteLine($"Texto: {busqueda.NombreTexto}");
            Console.WriteLine($"Palabra / oración de búsqueda: {busqueda.ConsultaBusqueda}");
            Console.WriteLine($"Tiempo de duración de la búsqueda: {busqueda.TiempoBusqueda.TotalMilliseconds} ms");
            Console.WriteLine($"Cantidad de apariciones: {busqueda.CantidadOcurrencias}\n");
        }
    }
}

class HistorialBusqueda
{
    public string Usuario { get; }
    public string NombreTexto { get; }
    public string ConsultaBusqueda { get; }
    public TimeSpan TiempoBusqueda { get; }
    public int CantidadOcurrencias { get; }

    public HistorialBusqueda(string usuario, string nombreTexto, string consultaBusqueda, TimeSpan tiempoBusqueda, int cantidadOcurrencias)
    {
        Usuario = usuario;
        NombreTexto = nombreTexto;
        ConsultaBusqueda = consultaBusqueda;
        TiempoBusqueda = tiempoBusqueda;
        CantidadOcurrencias = cantidadOcurrencias;
    }
}