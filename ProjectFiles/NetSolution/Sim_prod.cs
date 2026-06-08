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

public class Sim_prod : BaseNetLogic
{
    public override void Start()
    {
        runVariable = LogicObject.GetVariable("RunSimulation");

        // ==================================================
        // Variables de proceso
        // ==================================================

        Ley_mineral_entrada =LogicObject.GetVariable("Ley_mineral_entrada");

        Ley_mineral_salida =LogicObject.GetVariable("Ley_mineral_salida");

        Disponibilidad =LogicObject.GetVariable("Disponibilidad");

        Rendimiento =LogicObject.GetVariable("Rendimiento");

        Calidad =LogicObject.GetVariable("Calidad");

        Costo_total =LogicObject.GetVariable("Costo_total");

        Toneladas_producidas =LogicObject.GetVariable("Toneladas_producidas");
        
        Toneladas_producidas_molienda =LogicObject.GetVariable("Toneladas_producidas_molienda");

        // ==================================================
        // KPIs
        // ==================================================

        KPI_PROD_001 =LogicObject.GetVariable("KPI_PROD_001 PRODUCTION");

        KPI_PROD_002 =LogicObject.GetVariable("KPI_PROD_002 RECOVERY");

        KPI_PROD_003 =LogicObject.GetVariable("KPI_PROD_003 OEE");

        KPI_PROD_004 =LogicObject.GetVariable("KPI_PROD_004 COSTS PER TON");

        simulationTask =new PeriodicTask(Simulation, 250, LogicObject);

        simulationTask.Start();
    }

    private void Simulation()
    {
        if (runVariable.Value)
        {
            // ==================================================
            // Lectura de variables
            // ==================================================

            double Ley_mineral_entrada_val =Ley_mineral_entrada.Value;

            double Ley_mineral_salida_val =Ley_mineral_salida.Value;

            double Disponibilidad_val =Disponibilidad.Value;

            double Rendimiento_val =Rendimiento.Value;

            double Calidad_val =Calidad.Value;

            double Costo_total_val =Costo_total.Value;

            double Toneladas_producidas_val =Toneladas_producidas.Value;
            
            double Toneladas_producidas_molienda_val =Toneladas_producidas_molienda.Value;

            // ==================================================
            // KPI_PROD_001
            // Producción total
            // ==================================================

            KPI_PROD_001.Value =Toneladas_producidas_val;

            // ==================================================
            // KPI_PROD_002
            // Recovery (%)
            // Formula:
            // (Ley salida / Ley entrada) * 100
            // ==================================================
            if (Ley_mineral_entrada_val > 0 && Toneladas_producidas_molienda_val > 0)
            {
                KPI_PROD_002.Value =(Ley_mineral_salida_val* Toneladas_producidas_val /Ley_mineral_entrada_val* Toneladas_producidas_molienda_val) * 100.0;
            }
            else
            {
                KPI_PROD_002.Value = 0;
            }
            

            // ==================================================
            // KPI_PROD_003
            // OEE
            // Formula:
            // Disponibilidad * Rendimiento * Calidad
            // ==================================================

            KPI_PROD_003.Value =(Disponibilidad_val *Rendimiento_val *Calidad_val) / 10000.0;

            // ==================================================
            // KPI_PROD_004
            // Cost per ton
            // Formula:
            // Costo total / Toneladas producidas
            // ==================================================

            if (Toneladas_producidas_val > 0.001)
            {
                KPI_PROD_004.Value =Costo_total_val /Toneladas_producidas_val;
            }
            else
            {
                KPI_PROD_004.Value = 0;
            }
        }
    }

    public override void Stop()
    {
        simulationTask?.Dispose();
    }

    // ======================================================
    // Variables FTOptix
    // ======================================================

    private IUAVariable runVariable;

    private IUAVariable Ley_mineral_entrada;
    private IUAVariable Ley_mineral_salida;

    private IUAVariable Disponibilidad;
    private IUAVariable Rendimiento;
    private IUAVariable Calidad;

    private IUAVariable Costo_total;
    private IUAVariable Toneladas_producidas;
    private IUAVariable Toneladas_producidas_molienda;

    // ======================================================
    // KPIs
    // ======================================================

    private IUAVariable KPI_PROD_001;
    private IUAVariable KPI_PROD_002;
    private IUAVariable KPI_PROD_003;
    private IUAVariable KPI_PROD_004;

    // ======================================================
    // Variables auxiliares
    // ======================================================

    private double Ley_mineral_entrada_val = 0.0;
    private double Ley_mineral_salida_val = 0.0;

    private double Disponibilidad_val = 0.0;
    private double Rendimiento_val = 0.0;
    private double Calidad_val = 0.0;

    private double Costo_total_val = 0.0;
    private double Toneladas_producidas_val = 0.0;

    // ======================================================
    // Task
    // ======================================================

    private PeriodicTask simulationTask;
}