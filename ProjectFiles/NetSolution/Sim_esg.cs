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

public class Sim_esg : BaseNetLogic
{
    public override void Start()
    {
        runVariable = LogicObject.GetVariable("RunSimulation");

        CO2_total = LogicObject.GetVariable("CO2_total");
        Potencia_kW= LogicObject.GetVariable("Potencia_kW");
        Energia_kWh = LogicObject.GetVariable("Energia_kWh");
        Toneladas_producidas = LogicObject.GetVariable("Toneladas_producidas");
        Relaves_tratados = LogicObject.GetVariable("Relaves_tratados");
        Relaves_totales = LogicObject.GetVariable("Relaves_totales");
        Area_restaurada = LogicObject.GetVariable("Area_restaurada");
        Area_explotada = LogicObject.GetVariable("Area_explotada");
        Numero_incidentes = LogicObject.GetVariable("Numero_incidentes");
        Horas_trabajadas = LogicObject.GetVariable("Horas_trabajadas");
        Horas_capacitacion = LogicObject.GetVariable("Horas_capacitacion");
        Auditorias_aprobadas = LogicObject.GetVariable("Auditorias_aprobadas");
        Auditorias_totales = LogicObject.GetVariable("Auditorias_totales");

        KPI_ESG_001 = LogicObject.GetVariable("KPI_ESG_001 ENV INTENSITY");
        KPI_ESG_002 = LogicObject.GetVariable("KPI_ESG_002 REHABILITATION");
        KPI_ESG_003 = LogicObject.GetVariable("KPI_ESG_003 JOB SECURITY");
        KPI_ESG_004 = LogicObject.GetVariable("KPI_ESG_004 AUDIT COMPLIANCE");

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

            
            // Simulación base ESG
            
            double dt = .25;
            double toneladas = (20 + 5 * Math.Sin(decimalCounter / 4)) * estadoOperacion;
            if (toneladas < 0) toneladas = 0;
            Toneladas_producidas.Value += toneladas;
            
            
            double energia = Potencia_kW.Value * (dt / 3600.0);
            Energia_kWh.Value += energia;

            double co2 = energia * 0.45;
            CO2_total.Value += co2;

            double relaves = toneladas * 0.35;
            Relaves_totales.Value += relaves;

            double relavesTratados = relaves * (0.75 + 0.15 * Math.Abs(Math.Sin(decimalCounter / 5)));
            Relaves_tratados.Value += relavesTratados;

            double areaExp = 2 + Math.Abs(Math.Sin(decimalCounter / 6));
            Area_explotada.Value += areaExp;

            double areaRest = areaExp * (0.55 + 0.25 * Math.Abs(Math.Sin(decimalCounter / 7)));
            Area_restaurada.Value += areaRest;

            Horas_trabajadas.Value += 8;

            double horasCap = 1 + Math.Abs(Math.Sin(decimalCounter / 8));
            Horas_capacitacion.Value += horasCap;

            if (rand.NextDouble() < 0.02)
                Numero_incidentes.Value++;

            if (rand.NextDouble() < 0.03)
                Auditorias_totales.Value++;

            if (rand.NextDouble() < 0.025)
                Auditorias_aprobadas.Value++;

            
            // KPI-ESG-001
            // CO2e por tonelada
            

            if (Toneladas_producidas.Value > 0.001)
                KPI_ESG_001.Value = CO2_total.Value / Toneladas_producidas.Value;
            else
                KPI_ESG_001.Value = 0;

            
            // KPI-ESG-002
            // Relaves + rehabilitación
            

            double porcentajeRelaves = 0;
            double porcentajeRehabilitacion = 0;

            if (Relaves_totales.Value > 0.001)
                porcentajeRelaves = (Relaves_tratados.Value / Relaves_totales.Value) * 100;

            if (Area_explotada.Value > 0.001)
                porcentajeRehabilitacion = (Area_restaurada.Value / Area_explotada.Value) * 100;

            KPI_ESG_002.Value = (porcentajeRelaves + porcentajeRehabilitacion) / 2;

            
            // KPI-ESG-003
            // Seguridad laboral
            

            double tasaIncidentes = 0;
            double indiceCapacitacion = 0;

            if (Horas_trabajadas.Value > 0.001)
                tasaIncidentes = (Numero_incidentes.Value / Horas_trabajadas.Value) * 1000;

            if (Horas_trabajadas.Value > 0.001)
                indiceCapacitacion = (Horas_capacitacion.Value / Horas_trabajadas.Value) * 100;

            KPI_ESG_003.Value = indiceCapacitacion - tasaIncidentes;

            
            // KPI-ESG-004
            // Cumplimiento normativo
            

            if (Auditorias_totales.Value > 0.001)
                KPI_ESG_004.Value = (Auditorias_aprobadas.Value / Auditorias_totales.Value) * 100;
            else
                KPI_ESG_004.Value = 0;
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

    private IUAVariable CO2_total;
    private IUAVariable Energia_kWh;
    private IUAVariable Potencia_kW;
    private IUAVariable Toneladas_producidas;
    private IUAVariable Relaves_tratados;
    private IUAVariable Relaves_totales;
    private IUAVariable Area_restaurada;
    private IUAVariable Area_explotada;
    private IUAVariable Numero_incidentes;
    private IUAVariable Horas_trabajadas;
    private IUAVariable Horas_capacitacion;
    private IUAVariable Auditorias_aprobadas;
    private IUAVariable Auditorias_totales;

    private IUAVariable KPI_ESG_001;
    private IUAVariable KPI_ESG_002;
    private IUAVariable KPI_ESG_003;
    private IUAVariable KPI_ESG_004;
}