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

public class Sim_energía : BaseNetLogic
{
    public override void Start()
    {
        runVariable = LogicObject.GetVariable("RunSimulation");
        

        potencia = LogicObject.GetVariable("Potencia_kW");
        velocidad = LogicObject.GetVariable("Velocidad_Banda_mps");
        cargaBanda = LogicObject.GetVariable("Carga_Banda_kg_m");
        factorEmision = LogicObject.GetVariable("Factor_emision");
        flujo_material = LogicObject.GetVariable("flujo_material");
        potenciaRenovable = LogicObject.GetVariable("Potencia_Renovable_kW");
        toneladas_acumuladas = LogicObject.GetVariable("toneladas_acumuladas");
        energia = LogicObject.GetVariable("energia");
        energia_renovable = LogicObject.GetVariable("energia_renovable");

        KPI_ENER_1 = LogicObject.GetVariable("KPI_EN_001 CONSUMO");
        KPI_ENER_2 = LogicObject.GetVariable("KPI_EN_002 CO2_AREA");
        KPI_ENER_3 = LogicObject.GetVariable("KPI_EN_003 KWh_ton");
        KPI_ENER_4 = LogicObject.GetVariable("KPI_EN_004 % EN RENOVABLE");

        simulationTask = new PeriodicTask(Simulation, 250, LogicObject);
        simulationTask.Start();
    }

    private void Simulation()
    {
        if (runVariable.Value)
        {
            
            // KPI 1 (Consumo)
            KPI_ENER_1.Value = energia.Value;

            // KPI 2 (CO2)
            KPI_ENER_2.Value = energia.Value * factorEmision.Value;

            // KPI 3 (kWh/ton) 
            if (toneladas_acumuladas.Value > 0.001)
                KPI_ENER_3.Value = energia.Value / toneladas_acumuladas.Value;
            else
                KPI_ENER_3.Value = 0;


            // KPI 4 (% renovable)
            if (potencia.Value > 0.001)
                KPI_ENER_4.Value = (potenciaRenovable.Value / potencia.Value) * 100;
            else
                KPI_ENER_4.Value = 0;
        }
    }

    public override void Stop()
    {
        simulationTask?.Dispose();
    }
    
    private PeriodicTask simulationTask;
    private IUAVariable runVariable;
    private IUAVariable toneladas_acumuladas;
    private IUAVariable potencia;
    private IUAVariable velocidad;
    private IUAVariable cargaBanda;
    private IUAVariable factorEmision;
    private IUAVariable potenciaRenovable;
    private IUAVariable flujo_material;
    private IUAVariable energia;
    private IUAVariable energia_renovable;
   


    private IUAVariable KPI_ENER_1;
    private IUAVariable KPI_ENER_2;
    private IUAVariable KPI_ENER_3;
    private IUAVariable KPI_ENER_4;
}
