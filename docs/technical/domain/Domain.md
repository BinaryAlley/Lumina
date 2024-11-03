<span style="color: #FF0000;">Domain</span>  
├── <span style="color: #004BFF;">Audio Library Bounded Context</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;└── <span style="color: #F589DD;">Audio Library Aggregate</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;└── <span style="color: #BDE180;">Audio Metadata</span>  
├── <span style="color: #004BFF;">Video Library Bounded Context</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #F589DD;">Movie Library Aggregate</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #BDE180;">Movie Id</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #BDE180;">Video Metadata</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #FEB423;">Movie Library Service</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;└── <span style="color: #B46DD4;">Movie</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #F589DD;">Tv Show Library Aggregate</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #BDE180;">Episode Id</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #BDE180;">Season Id</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #BDE180;">Tv Show Id</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #80B0E0;">Episode</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #80B0E0;">Season</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #FEB423;">Tv Show Library Service</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;└── <span style="color: #B46DD4;">Tv Show</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;└── <span style="color: #FEB423;">Video Library Service</span>  
├── <span style="color: #004BFF;">Photo Library Bounded Context</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;└── <span style="color: #F589DD;">Photo Library Aggregate</span>  
├── <span style="color: #004BFF;">Written Content Library Bounded Context</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #F589DD;">Book Library Aggregate</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #BDE180;">Book Id</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #BDE180;">Book Rating</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #BDE180;">Book Series Id</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #BDE180;">ISBN</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #BDE180;">Written Content Metadata</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #80B0E0;">Book Series</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #FEB423;">Book Library Service</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;└── <span style="color: #B46DD4;">Book</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #F589DD;">Magazine Library Aggregate</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;└── <span style="color: #B46DD4;">Magazine</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;└── <span style="color: #FEB423;">Written Content Library Service</span>  
├── <span style="color: #004BFF;">Media Contributor Bounded Context</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;└── <span style="color: #F589DD;">Media Contributor Aggregate</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #BDE180;">Contributor Name</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #BDE180;">Media Contributor Id</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #BDE180;">Media Contributor Role</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;└── <span style="color: #B46DD4;">Media Contributor</span>  
├── <span style="color: #004BFF;">Library Management Bounded Context</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;└── <span style="color: #F589DD;">Library Aggregate</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #BDE180;">Library Id</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;└── <span style="color: #B46DD4;">Library</span>  
├── <span style="color: #004BFF;">User Management Bounded Context</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;└── <span style="color: #F589DD;">User Aggregate</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #BDE180;">User Id</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;└── <span style="color: #B46DD4;">User</span>  
├── <span style="color: #004BFF;">File System Management Bounded Context</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;└── <span style="color: #F589DD;">File System Management Aggregate</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #BDE180;">File System Path Id</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #BDE180;">Path Segment</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #BDE180;">Stream Info</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #BDE180;">Thumbnail</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #80B0E0;">Directory</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #80B0E0;">File</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #80B0E0;">UNIX Root Item</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #80B0E0;">Windows Root Item</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #FEB423;">Directory Provider Service</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #FEB423;">File Provider Service</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #FEB423;">Directory Service</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #FEB423;">File Service</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #FEB423;">Drive Service</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #FEB423;">Path Service</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #FEB423;">Thumbnail Service</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #FEB423;">File System Permissions Service</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── <span style="color: #FEB423;">File Type Service</span>  
│&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;└── <span style="color: #B46DD4;">File System Item</span>  

<span style="color: #004BFF;">&bull; Bounded Context</span>  
<span style="color: #F589DD;">&bull; Aggregate</span>  
<span style="color: #B46DD4;">&bull; Aggregate Root</span>  
<span style="color: #80B0E0;">&bull; Entity</span>  
<span style="color: #BDE180;">&bull; Value Object</span>  
<span style="color: #E14B5B;">&bull; Event</span>  
<span style="color: #FEB423;">&bull; Service</span>  