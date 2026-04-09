#region Using directives
using System;
using UAManagedCore;
using FTOptix.NetLogic;
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
        flujo = LogicObject.GetVariable("Flujo_ton_h");
        factorEmision = LogicObject.GetVariable("Factor_emision");
        renovable = LogicObject.GetVariable("Porcentaje_renovable");

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
            potencia.Value = basePower * estadoOperacion + ruidoPotencia;

            double flujoMax = 100;
            double ruidoFlujo = (rand.NextDouble() - 0.5) * 10;
            flujo.Value = flujoMax * carga * estadoOperacion + ruidoFlujo;

            double baseFactor = 0.45;
            double variacion = (rand.NextDouble() - 0.5) * 0.02;
            factorEmision.Value = baseFactor + variacion;

            double cambio = (rand.NextDouble() - 0.5) * 0.5;
            double nuevo = renovable.Value + cambio;

            if (nuevo < 10) nuevo = 10;
            if (nuevo > 50) nuevo = 50;

            renovable.Value = nuevo;
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
    private IUAVariable flujo;
    private IUAVariable factorEmision;
    private IUAVariable renovable;
}