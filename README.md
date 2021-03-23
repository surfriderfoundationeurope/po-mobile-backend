# po-mobile-backend

<!--- OPTIONAL: You can add badges and shields to reflect the current status of the project, the licence it uses and if any dependencies it uses are up-to-date. Plus they look pretty cool! You can find a list of badges or design your own at https://shields.io/ --->

> *Proudly Powered by [Surfrider Foundation Europe](https://surfrider.eu/), this initiative is a part of [the Plastic Origin Project](https://www.plasticorigins.eu/) - a citizen science project that aims at mapping plastic pollution in european rivers and provide data to all stakeholders.\
> Browse [@The-Plastic-Origins-Project](https://github.com/surfriderfoundationeurope/The-Plastic-Origins-Project) to know more about the project's initiatives*.
_________________

Welcome to the Plastic Origins 'all in one' backend that allows the Plastic Origins Mobile app (available on [Android](https://play.google.com/store/apps/details?id=com.plasticorigins&hl=fr&gl=US) & [IOS](https://apps.apple.com/fr/app/plastic-origins/id1532710998)) and Web app (data labelling tool  [www.trashroulette.com](https://www.trashroulette.com/#/)) to consume their main APIs that support:

* User CRUD on our PostgreSQL database.

* Upload images (images to be labelled though our data labelling tool [www.trashroulette.com](https://www.trashroulette.com/#/)).

* Upload video (videos to be analysed by our AI litter detection model).

* Parse JSON files coming from our Plastic Origins Mobil app and store data in our PostgreSQL database.

* Read and update images listed in the label scheme of our PostgreSQL database.

## **Getting Started**

### **Prerequisites**

Before you begin, ensure you have met the following requirements:

* You have installed [`.Net Core 3.1 or lastest`](https://dotnet.microsoft.com/download/dotnet/3.1)
* You have installed the latest version of [`Azure Emulator`](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator) if you want to use on your local machine
* You have a `PostgreSQL 11.6 minimum` database for local use on your machine.

#### **Technical stack**

* Language: `C#`
* Framework: `.Net Core`
* Unit test framework: `XUnit`

## **Settings file**

Create a `local.settings.json` within the `functions` folder. Paste this content and obviously replace with your values.

```json
{
	"IsEncrypted": false,
	"Values": {
		"AzureWebJobsTraceStorage": "UseDevelopmentStorage=true;",
		"AzureWebJobsStorage": "UseDevelopmentStorage=true;",
		"JwtTokenKey": "jsutAVeryLongRandomString",
		"PostgresqlDbConnectionString": "",
		"MailjetApiKey": "",
		"MailjetApiSecret": "",
		"BaseFunctionUrl": "http://localhost:7071"
	},
	"Host": {
		"CORS": "*"
	}
	"Host": {
		"CORS": "*"
	}
}
```

Remarks: MailjetApiKeys are only required for sending emails.

### **API references**

*SOON: To see API specification used by this repository browse to the Swagger documentation (currently not available).*

<!--- Below an example of the API Functions to use for creating po-mobil-backend Swagger documentation:

```http
AnnotateImage:[POST] /images/annotate
```

```http
GetImageBBox:[GET] /images/bbox{imageId}
```

```http
GetImageTrashTypes:[GET] /images/trashtypes
```

```http
GetOneImage:[GET] /images/imgName/{fileName}
```

```http
GetRandomImage:[GET] /images/random
```

```http
Heartbeat:[GET,POST] /heartbeat
```

```http
Login:[POST] /login
```

```http
ReferenceGetRiverDB:[GET] /reference/rivers
```

```http
RefreshToken:[POST] /auth/refreshtoken
```

```http
Register:[POST] /register
```

```http
ResetAccount:[POST] /auth/reset
```

```http
ResetAccountForm:[GET] /auth/reset
```

```http
UpdateImageData:[POST] /images/update
```

```http
UpdatePassword:[POST] /auth/updatepassword
```

```http
UploadTrace:[POST] /trace
```

```http
UploadTraceAttachment:[POST] /trace/{traceId}/attachments/{fileName}
```

```http
Validate:[GET] /validate/{code}
```
-->

## **Build and Test**

### **Build the Project**

```shell
dotnet restore 
```
```shell
dotnet build
```

### **Launch the program**

```shell
dotnet run 
```
```shell
http://localhost:7071/
```

### **Test**

```shell
dotnet test
```

## **Contributing**

It's great to have you here! We welcome any help and thank you in advance for your contributions.

* Feel free to **report a problem/bug** or **propose an improvement** by creating a [new issue](https://github.com/surfriderfoundationeurope/po-mobile-backend/issues). Please document as much as possible the steps to reproduce your problem (even better with screenshots). If you think you discovered a security vulnerability, please contact directly our [Maintainers](##Maintainers).

* Take a look at the [open issues](https://github.com/surfriderfoundationeurope/po-mobile-backend/issues) labeled as **`help wanted`**, feel free to **comment** to share your ideas or **submit a** [**pull request**](https://github.com/surfriderfoundationeurope/po-mobile-backend/pulls) if you feel that you can fix the issue yourself. Please document any relevant changes.

## **Maintainers**

If you experience any problems, please don't hesitate to ping:

* [@cmaneu](https://github.com/cmaneu)
* [@benzinamohamedelyes](https://github.com/benzinamohamedelyes)
* [@Vincent-Guiberteau](https://github.com/Vincent-Guiberteau)

Special thanks to all our [Contributors](https://github.com/surfriderfoundationeurope/The-Plastic-Origins-Project).

## **License**

We’re using the [GNU General Public License (GPL) version 3 - `GPLv3`](https://www.gnu.org/licenses/gpl-3.0.en.html) - a free, copyleft license for software and other kinds of works.
