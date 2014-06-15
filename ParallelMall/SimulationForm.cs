﻿/*
 * Utworzone przez SharpDevelop.
 * Użytkownik: michal
 * Data: 2014-06-14
 * Godzina: 13:33
 * 
 * Do zmiany tego szablonu użyj Narzędzia | Opcje | Kodowanie | Edycja Nagłówków Standardowych.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace ParallelMall
{
	/// <summary>
	/// Description of SimulationForm.
	/// </summary>
	public partial class SimulationForm : Form
	{
		private int numberCases;
		private int numberProductTypes;
		private int numberOfProducts;
		private Thread clientsGeneration;
        private Thread casesWatching;
		private List<Case> cases;
        private List<List<Client>> queues; //Indeksy odpowiadają indeksom półek
        private List<Thread> productsConsumption;

        //Zwraca listę typów produktów, których już nie ma na półce
        private List<int> checkIfCaseIsEmpty(Case c)
        {
            List<int> productTypes = new List<int>();
            for (int i = 0; i < numberProductTypes; i++)
            {
                if (c.GetProductCount(i) <= 0) productTypes.Add(i);
            }
            return productTypes;
        }

        private void watchCases()
        {
            while(true)
            {
                for (int caseIndex = 0; caseIndex < numberCases; caseIndex++)
                {
                    List<int> emptyCases = checkIfCaseIsEmpty(cases[caseIndex]);
                    if (emptyCases.Count != 0)
                    {
                        cases[caseIndex].RefillingPreparation = true;
                        while (cases[caseIndex].Taking) Thread.Sleep(1); //Oczekiwanie aż klienci odpuszczą
                        foreach (int i in emptyCases)
                        {
                            cases[caseIndex].RefillProducts(i);
                        }
                    }
                    else Thread.Sleep(1000);
                }
            }
        }

		private void generateClients()
		{
            
			Random rand = new Random();
			while (true)
			{
				int time = rand.Next(500 / numberCases * 3, 1300 / numberCases * 3); //Czas oczekiwania na dołączenie kolejnego klienta
				Thread.Sleep(time);
				int queue = rand.Next(0, numberCases); //Numer kolejki do dołączenia się
                int productType = rand.Next(0, numberProductTypes);
                Client c = new Client(productType);
                queues[queue].Add(c);
                lblClientsIndicator.Text = "Clients: " + queues[0].Count.ToString() + " " + queues[1].Count.ToString() + "" +queues[2].Count.ToString();
			}
		}

        private void consumeProducts(object queueObject)
        {
            int queue = (int)queueObject;
            while (true)
            {
                if (queues[queue].Count != 0)
                {
                    Thread.Sleep(500);
                    while (cases[queue].RefillingPreparation || cases[queue].Refilling) Thread.Sleep(1); //Łapy precz - pan chce nałożyć produkty
                    Client currentClient = queues[queue][0]; //Pierwszy w kolejce
                    while (cases[queue].GetProductCount(currentClient.ProductType) <= 0) Thread.Sleep(1);
                    cases[queue].TakeProduct(currentClient.ProductType);
                    queues[queue].RemoveAt(0);
                    //lblClientsIndicator.Text = "Clients: " + clientQueue.Count;
                }
                else Thread.Sleep(1);
            }
        }
		
		private void initializeVisualisation()
		{
			cases = new List<Case>();
			for (int i = 0; i < numberCases; i++)
			{
				Case c = new Case(i + 1, numberProductTypes, numberOfProducts);
				c.Location = new System.Drawing.Point(i *150, 5);
				this.Controls.Add(c);
				cases.Add(c);
				if (i == numberCases - 1)
				{
					this.Size = new Size((i + 1) * 155, 200);
				}
			}
		}

        public void initializeSimulation()
        {
            queues = new List<List<Client>>();
            for (int i = 0; i < numberCases; i++)
            {
                List<Client> queue = new List<Client>();
                queues.Add(queue);
            }

            casesWatching = new Thread(new ThreadStart(watchCases));
            casesWatching.Start();

            clientsGeneration = new Thread(new ThreadStart(generateClients));
            clientsGeneration.Start();

            productsConsumption = new List<Thread>();
            for (int i = 0; i < numberCases; i++)
            {
                Thread t = new Thread(new ParameterizedThreadStart(consumeProducts));
                t.Start(i);
                productsConsumption.Add(t);
            }
        }
		
		public SimulationForm(int casesCount, int typesCount, int productCount)
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			this.numberCases = casesCount;
			this.numberProductTypes = typesCount;
			this.numberOfProducts = productCount;
			
			initializeVisualisation();
            initializeSimulation();
		}
	}
}
