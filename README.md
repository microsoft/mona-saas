# Mona for SaaS

In the dynamic world of SaaS, every action—whether it's a new customer signing up, an existing one renewing, upgrading, or even canceling—creates a pulse that keeps your business moving. Mona is designed to capture this rhythm seamlessly. By connecting these Marketplace events to an Azure Event Grid topic within your cloud environment, Mona simplifies the automation of essential workflows. This allows you to scale effortlessly and concentrate on delivering exceptional value to your customers.

## Quickstart

### Clone this repo

[Open an **Azure Cloud Shell**, choose the **Bash** experience](https://learn.microsoft.com/azure/cloud-shell/get-started/ephemeral?tabs=azurecli#start-cloud-shell), and clone this repo. Navigate to the setup folder.

```bash
git clone https://github.com/microsoft/mona-saas
cd mona-saas/Mona.SaaS/Mona.SaaS.Setup
```

### Run the setup script

Run the `basic-deploy.sh` script to deploy Mona into your Azure environment. You'll need to provide two parameters:

* **Azure Location `(-r)`**: [The Azure region](https://azure.microsoft.com/explore/global-infrastructure/geographies/) where Mona will be deployed (e.g., `eastus`, `southeastasia`).
* **Deployment Name `(-n`)**: A unique name fo your Mona deployment. It must be 5-13 alphanumeric characters (e.g., `monatest01`).

> For a complete list of Azure regions to choose from, run `az account list-locations -o table`.

#### Example

```bash
./basic-deploy.sh -r "eastus" -n "monatest01"
```

### Finish setting up Mona

Click the link provided by the setup script to complete the setup. You will be redirected the Mona admin app running in your environment. At no point are you leaving your Azure environment.
