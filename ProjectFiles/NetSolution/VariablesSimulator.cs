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
        potenciaRenovable = LogicObject.GetVariable("Potencia_Renovable_kW");

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

            ramp.Value = integerCounter;
            sine.Value = Math.Sin(decimalCounter) * 100;
            cosine.Value = Math.Cos(decimalCounter) * 50;

            estadoOperacion = (rand.NextDouble() > 0.05) ? 1.0 : 0.0;
            carga = 0.5 + 0.3 * Math.Sin(decimalCounter / 5);

            double basePower = 80 + (carga * 40);
            double ruidoPotencia = (rand.NextDouble() - 0.5) * 5;
            potencia.Value = (basePower + ruidoPotencia) * estadoOperacion;

            double baseVelocidad = 1.5 + 0.3 * Math.Sin(decimalCounter / 3);
            double ruidoVelocidad = (rand.NextDouble() - 0.5) * 0.2;
            velocidad.Value = (baseVelocidad + ruidoVelocidad) * estadoOperacion;

            double baseCarga = 100 + (carga * 50);
            double ruidoCarga = (rand.NextDouble() - 0.5) * 10;
            cargaBanda.Value = (baseCarga + ruidoCarga) * estadoOperacion;

            //flujo.Value = velocidad.Value * cargaBanda.Value * 3.6;

            double baseFactor = 0.45;
            double variacion = (rand.NextDouble() - 0.5) * 0.02;
            factorEmision.Value = baseFactor + variacion;

            double porcentajeRenovable = 0.2 + 0.2 * Math.Sin(decimalCounter / 10);
            potenciaRenovable.Value = potencia.Value * porcentajeRenovable;
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

    private IUAVariable runVariable;
    private IUAVariable sine;
    private IUAVariable cosine;
    private IUAVariable ramp;

    private IUAVariable potencia;
    private IUAVariable velocidad;
    private IUAVariable cargaBanda;
    private IUAVariable factorEmision;
    private IUAVariable potenciaRenovable;
}
