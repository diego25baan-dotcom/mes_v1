#region Using directives
using System;
using UAManagedCore;
using FTOptix.NetLogic;
using FTOptix.Alarm;
using FTOptix.SerialPort;
using FTOptix.EventLogger;
using FTOptix.InfluxDBStore;
using FTOptix.InfluxDBStoreRemote;
using FTOptix.RAEtherNetIP;
using FTOptix.CommunicationDriver;
using FTOptix.Report;
#endregion

public class Sim_agua : BaseNetLogic
{
    public override void Start()
    {
        runVariable = LogicObject.GetVariable("RunSimulation");

        caudalAgua = LogicObject.GetVariable("Caudal_Agua_Ls");
        caudalDescarga = LogicObject.GetVariable("Caudal_descarga_m3h");
        agua_reutilizada = LogicObject.GetVariable("Agua_reutilizada");
        agua_total = LogicObject.GetVariable("Agua_total");
        water_consumption = LogicObject.GetVariable("Water_consumption");
        numero_incidentes = LogicObject.GetVariable("Numero_incidentes");
        tiempo_respuesta = LogicObject.GetVariable("Tiempo_respuesta");
        toneladas_acumuladas = LogicObject.GetVariable("Toneladas_acumuladas");

        KPI_WA_001 = LogicObject.GetVariable("KPI_WA_001_huella");
        KPI_WA_002 = LogicObject.GetVariable("KPI_WA_002_reutilizacion");
        KPI_WA_003 = LogicObject.GetVariable("KPI_WA_003_des_residual");
        KPI_WA_004 = LogicObject.GetVariable("KPI_WA_004_incidentes");

        simulationTask = new PeriodicTask(Simulation, 250, LogicObject);
        simulationTask.Start();
    }

    private void Simulation()
    {
        if (runVariable.Value)
        {
            // Contadores
            if (integerCounter <= 99)
                integerCounter++;
            else
                integerCounter = 0;

            decimalCounter += 0.05;

            // Estado aleatorio operación
            estadoOperacion =
                (rand.NextDouble() > 0.05) ? 1.0 : 0.0;

            // Lectura caudal agua
            caudal_Agua_val = caudalAgua.Value;

            if (caudal_Agua_val < 0)
                caudal_Agua_val = 0;

            caudalAgua.Value = caudal_Agua_val;

            // Tiempo de muestreo
            double dt = 0.25;

            // Lectura variables
            toneladas_acumuladas_val =
                toneladas_acumuladas.Value;

            agua_total_val =
                agua_total.Value;

            agua_reutilizada_val =
                agua_reutilizada.Value;

            // ==================================================
            // KPI-WA-001: Huella hídrica
            // ==================================================

            if (toneladas_acumuladas_val > 0.001)
            {
                KPI_WA_001.Value =
                    agua_total_val /
                    toneladas_acumuladas_val;
            }
            else
            {
                KPI_WA_001.Value = 0;
            }

            // ==================================================
            // KPI-WA-002: % reutilización
            // ==================================================

            if (agua_total_val > 0.001)
            {
                KPI_WA_002.Value =
                    (agua_reutilizada_val /
                    agua_total_val) * 100;
            }
            else
            {
                KPI_WA_002.Value = 0;
            }

            // ==================================================
            // KPI-WA-003: Descarga residual acumulada
            // ==================================================

            caudalDescarga_val =
                caudalDescarga.Value;

            double descarga_m3 =
                caudalDescarga_val *
                (dt / 3600.0);

            descarga_acumulada += descarga_m3;

            KPI_WA_003.Value =
                descarga_acumulada;

            // ==================================================
            // KPI-WA-004:
            // Tiempo promedio de respuesta incidentes
            // ==================================================

            // Simulación aleatoria de incidente
            if (rand.NextDouble() < 0.02)
            {
                numero_incidentes.Value++;

                // Tiempo respuesta entre 5 y 30 min
                tiempo_respuesta.Value =
                    5 + rand.NextDouble() * 25;

                tiempo_respuesta_total +=
                    tiempo_respuesta.Value;
            }

            // Promedio tiempo respuesta
            if (numero_incidentes.Value > 0)
            {
                KPI_WA_004.Value =
                    tiempo_respuesta_total /
                    numero_incidentes.Value;
            }
            else
            {
                KPI_WA_004.Value = 0;
            }
        }
    }

    public override void Stop()
    {
        simulationTask?.Dispose();
    }

    // ======================================================
    // Variables internas
    // ======================================================

    private int integerCounter;
    private double decimalCounter;
    private double estadoOperacion;

    private Random rand = new Random();

    private PeriodicTask simulationTask;

    // ======================================================
    // Variables FTOptix
    // ======================================================

    private IUAVariable runVariable;

    private IUAVariable caudalAgua;
    private IUAVariable caudalDescarga;

    private IUAVariable agua_reutilizada;
    private IUAVariable agua_total;

    private IUAVariable water_consumption;

    private IUAVariable numero_incidentes;
    private IUAVariable tiempo_respuesta;

    private IUAVariable toneladas_acumuladas;

    // ======================================================
    // KPIs
    // ======================================================

    private IUAVariable KPI_WA_001;
    private IUAVariable KPI_WA_002;
    private IUAVariable KPI_WA_003;
    private IUAVariable KPI_WA_004;

    // ======================================================
    // Variables auxiliares
    // ======================================================

    private double agua_total_val = 0.0;
    private double agua_reutilizada_val = 0.0;

    private double caudal_Agua_val = 0.0;
    private double caudalDescarga_val = 0.0;

    private double descarga_acumulada = 0.0;

    private double tiempo_respuesta_total = 0.0;

    private double toneladas_acumuladas_val = 0.0;
}