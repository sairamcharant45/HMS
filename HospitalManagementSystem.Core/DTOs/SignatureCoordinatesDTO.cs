using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Core.DTOs
{
    public class SignatureCoordinatesDTO
    {
        public string documentType { get; set; }
        public string subscriberUniqueId { get; set; }
        public placeHolderCoordinates placeHolderCoordinates { get; set; }
        public esealplaceHolderCoordinates esealPlaceHolderCoordinates { get; set; }

        public string id { get; set; }


    }
    public class placeHolderCoordinates
    {
        public string pageNumber { get; set; }

        public string signatureXaxis { get; set; }

        public string signatureYaxis { get; set; }

        public string imgHeight { get; set; }

        public string imgWidth { get; set; }
    }

    public class esealplaceHolderCoordinates
    {
        public string pageNumber { get; set; }

        public string signatureXaxis { get; set; }

        public string signatureYaxis { get; set; }

        public string imgHeight { get; set; }

        public string imgWidth { get; set; }
    }
}
