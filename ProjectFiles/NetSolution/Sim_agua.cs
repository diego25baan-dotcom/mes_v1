#region Using directives
using System;
using UAManagedCore;
using FTOptix.NetLogic;
using FTOptix.Alarm;
using FTOptix.SerialPort;
using FTOptix.EventLogger;
using FTOptix.InfluxDBStore;
using FTOptix.InfluxDBStoreRemote;
#endregion

public class Sim_agua : BaseNetLogic
{
    public override void Start()
    {
        runVariable = LogicObject.GetVariable("RunSimulation");

        caudalAgua = LogicObject.GetVariable("Caudal_Agua_Ls");
        caudalDescarga = LogicObject.GetVariable("Caudal_descarga_m3h");
        agua_reutilizada = LogicObject.GetVariable("Agua_reutilizada");
        agua_total= LogicObject.GetVariable("Agua_total");
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
            if (integerCounter <= 99)
                integerCounter++;
            else
                integerCounter = 0;

            decimalCounter += 0.05;

            estadoOperacion = (rand.NextDouble() > 0.05) ? 1.0 : 0.0;
            


            double baseCaudalAgua = 50 + 10 * Math.Sin(decimalCounter / 4);
            double ruidoAgua = (rand.NextDouble() - 0.5) * 5;
            double caudalAgua_val = (baseCaudalAgua + ruidoAgua) * estadoOperacion;

            if (caudalAgua_val < 0) caudalAgua_val = 0;

            caudalAgua.Value = caudalAgua_val;

            
            double dt = 1; 
            double volumen_agua = caudalAgua_val * dt * 0.001;

            
            agua_acumulada += volumen_agua;
            agua_total.Value += volumen_agua;

            // reutilización (40–80%)
            double factorReuso = 0.4 + 0.4 * Math.Abs(Math.Sin(decimalCounter / 6));
            double agua_reuso = volumen_agua * factorReuso;
            agua_reutilizada.Value += agua_reuso;

            // KPI-WA-001: Huella hídrica
            if (toneladas_acumuladas.Value > 0.001)
                KPI_WA_001.Value = agua_acumulada / toneladas_acumuladas.Value;
            else
                KPI_WA_001.Value = 0;

            // 🔹 KPI-WA-002: % reutilización
            if (agua_acumulada > 0.001)
                KPI_WA_002.Value = (agua_reutilizada.Value / agua_acumulada) * 100;
            else
                KPI_WA_002.Value = 0;


            // 🔷 KPI-WA-003: Descarga residual

            double baseDescarga = 20 + 5 * Math.Sin(decimalCounter / 3);
            double caudalDescarga_val = baseDescarga * estadoOperacion;

            if (caudalDescarga_val < 0) caudalDescarga_val = 0;

            caudalDescarga.Value = caudalDescarga_val;

            // m3/h → m3 en dt
            double descarga_m3 = caudalDescarga_val * (dt / 3600.0);
            descarga_acumulada += descarga_m3;

            KPI_WA_003.Value = descarga_acumulada;


            // 🔷 KPI-WA-004: Incidentes y tiempo de respuesta

            // Simular incidente aleatorio
            if (rand.NextDouble() < 0.02)
            {
                numero_incidentes.Value++;

                // tiempo de respuesta entre 5 y 30 min
                tiempo_respuesta.Value = 5 + rand.NextDouble() * 25;

                tiempo_respuesta_total += tiempo_respuesta.Value;
            }

            // KPI: tiempo promedio de respuesta
            if (numero_incidentes.Value > 0)
                KPI_WA_004.Value = tiempo_respuesta_total / numero_incidentes.Value;
            else
                KPI_WA_004.Value = 0;
        }
    }

    public override void Stop()
    {
        simulationTask?.Dispose();
    }
    private int integerCounter;
    private double decimalCounter;
    private double estadoOperacion;
    private IUAVariable runVariable;
    private Random rand = new Random();
    private PeriodicTask simulationTask;
    private IUAVariable caudalAgua;
    private IUAVariable caudalDescarga;
    private IUAVariable agua_reutilizada;
    private IUAVariable agua_total;
    private IUAVariable numero_incidentes;
    private IUAVariable tiempo_respuesta;
    private IUAVariable toneladas_acumuladas;

    private IUAVariable KPI_WA_001;
    private IUAVariable KPI_WA_002;
    private IUAVariable KPI_WA_003;
    private IUAVariable KPI_WA_004;

    
    private double agua_acumulada = 0.0;
    private double descarga_acumulada = 0.0;
    private double tiempo_respuesta_total = 0.0;
}
