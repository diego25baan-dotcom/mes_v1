#region Using directives
using System;
using UAManagedCore;
using FTOptix.NetLogic;
using FTOptix.Alarm;
using FTOptix.SerialPort;
using FTOptix.EventLogger;
#endregion

public class VariablesSimulator : BaseNetLogic
{
    public override void Start()
    {
        runVariable = LogicObject.GetVariable("RunSimulation");
        sine = LogicObject.GetVariable("Sine");
        ramp = LogicObject.GetVariable("Ramp");
        cosine = LogicObject.GetVariable("Cosine");

        potencia = LogicObject.GetVariable("Potencia_kW");
        velocidad = LogicObject.GetVariable("Velocidad_Banda_mps");
        cargaBanda = LogicObject.GetVariable("Carga_Banda_kg_m");
        factorEmision = LogicObject.GetVariable("Factor_emision");
        flujo_material = LogicObject.GetVariable("flujo_material");
        potenciaRenovable = LogicObject.GetVariable("Potencia_Renovable_kW");

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
            // 🔹 Contadores
            if (integerCounter <= 99)
                integerCounter++;
            else
                integerCounter = 0;

            decimalCounter += 0.05;

            ramp.Value = integerCounter;
            sine.Value = Math.Sin(decimalCounter) * 100;
            cosine.Value = Math.Cos(decimalCounter) * 50;

            // 🔹 Estado del sistema
            estadoOperacion = (rand.NextDouble() > 0.05) ? 1.0 : 0.0;
            carga = 0.5 + 0.3 * Math.Sin(decimalCounter / 5);

            // 🔹 POTENCIA (kW)
            double basePower = 80 + (carga * 40);
            double ruidoPotencia = (rand.NextDouble() - 0.5) * 5;
            double potencia_val = (basePower + ruidoPotencia) * estadoOperacion;
            potencia.Value = potencia_val;

            // 🔹 VELOCIDAD (m/s)
            double baseVelocidad = 1.5 + 0.3 * Math.Sin(decimalCounter / 3);
            double ruidoVelocidad = (rand.NextDouble() - 0.5) * 0.2;
            double velocidad_val = (baseVelocidad + ruidoVelocidad) * estadoOperacion;
            velocidad.Value = velocidad_val;

            // 🔹 CARGA (kg/m)
            double baseCarga = (100 + (carga * 50)) * 1000;
            double ruidoCarga = ((rand.NextDouble() - 0.5) * 10) * 1000;
            double carga_val = ((baseCarga + ruidoCarga) * estadoOperacion) / 1000;
            cargaBanda.Value = carga_val;

            // 🔹 FLUJO (kg/s)
            double flujo_val = velocidad_val * carga_val;
            flujo_material.Value = flujo_val;

            // 🔹 TIEMPO DE MUESTREO (250 ms)
            double dt = 1;

            // 🔹 TONELADAS (ton)
            double toneladas_procesadas = (flujo_val * dt) / 1000.0;

            // 🔹 ENERGÍA (kWh)
            double energia_kWh = potencia_val * (dt / 3600.0);

            // 🔹 ACUMULADORES (🔥 NUEVO)
            energia_acumulada += energia_kWh;
            toneladas_acumuladas += toneladas_procesadas;

            // 🔹 FACTOR DE EMISIÓN
            double baseFactor = 0.45;
            double variacion = (rand.NextDouble() - 0.5) * 0.02;
            double factor_val = baseFactor + variacion;
            factorEmision.Value = factor_val;

            // 🔹 ENERGÍA RENOVABLE
            double porcentajeRenovable = 0.2 + 0.2 * Math.Sin(decimalCounter / 10);
            double potenciaRenovable_val = potencia_val * porcentajeRenovable;
            potenciaRenovable.Value = potenciaRenovable_val;

            // 🔹 KPI 1 (Consumo)
            KPI_ENER_1.Value = potencia_val;

            // 🔹 KPI 2 (CO2)
            KPI_ENER_2.Value = potencia_val * factor_val;

            // 🔹 KPI 3 (kWh/ton) 🔥 ACUMULADO REAL
            if (toneladas_acumuladas > 0.001)
                KPI_ENER_3.Value = energia_acumulada / toneladas_acumuladas;
            else
                KPI_ENER_3.Value = 0;

            // 🔹 KPI 4 (% renovable)
            if (potencia_val > 0.001)
                KPI_ENER_4.Value = (potenciaRenovable_val / potencia_val) * 100;
            else
                KPI_ENER_4.Value = 0;
        }
    }

    public override void Stop()
    {
        simulationTask?.Dispose();
    }

    private PeriodicTask simulationTask;
    private int integerCounter;
    private double decimalCounter;

    private double carga;
    private double estadoOperacion;
    private Random rand = new Random();

    // 🔥 NUEVAS VARIABLES ACUMULADORAS
    private double energia_acumulada = 0.0;
    private double toneladas_acumuladas = 0.0;

    private IUAVariable runVariable;
    private IUAVariable sine;
    private IUAVariable cosine;
    private IUAVariable ramp;

    private IUAVariable potencia;
    private IUAVariable velocidad;
    private IUAVariable cargaBanda;
    private IUAVariable factorEmision;
    private IUAVariable potenciaRenovable;
    private IUAVariable flujo_material;

    private IUAVariable KPI_ENER_1;
    private IUAVariable KPI_ENER_2;
    private IUAVariable KPI_ENER_3;
    private IUAVariable KPI_ENER_4;
}