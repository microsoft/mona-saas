#!/bin/bash

aad_app_name="BohdanTestApp_"$(date +"%M%S")
aad_app_secret=$(openssl rand -base64 64)

echo "### $aad_app_name ###"


graph_token=$(az account get-access-token --resource-type ms-graph --query accessToken --output tsv)
create_app_resp=$(curl -X POST \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $graph_token" \
    -d '{ 
            "displayName": "'$aad_app_name'",
            "appRoles": [
                {
                    "allowedMemberTypes": ["User"],
                    "description": "Mona administrators can access the Mona admin center and use the various test endpoints.",
                    "displayName": "Mona Administrators",
                    "id": "'$(uuidgen)'",
                    "isEnabled": true,
                    "value": "monaadmins"
                }
            ],
            "optionalClaims": {
                "idToken": [
                    {
                        "name": "given_name",
                        "source": null,
                        "essential": false,
                        "additionalProperties": []
                    }
                ],
                "accessToken": [],
                "saml2Token": []
            },
            "requiredResourceAccess": [
                {
                    "resourceAppId": "00000003-0000-0000-c000-000000000000",
                    "resourceAccess": [
                    {
                        "id": "e1fe6dd8-ba31-4d61-89e7-88639da4683d",
                        "type": "Scope"
                    },
                    {
                        "id": "14dad69e-099b-42c9-810b-d002981feec1",
                        "type": "Scope"
                    }
                    ]
                }
            ]
        }' \
    "https://graph.microsoft.com/v1.0/applications"  ) #| jq -r '.appId' 
echo $create_app_resp
aad_obj_id=$( echo $create_app_resp | jq -r '.id' )
aad_app_id=$( echo $create_app_resp | jq -r '.appId' )
echo $aad_obj_id
echo $aad_app_id

aad_app_credentials_res=$(curl -X POST \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $graph_token" \
    -d '{ 
            "displayName": "MonaCredentials",
            "endDateTime": "2299-12-31",
            "startDateTime": "2000-12-31"
        }' \
    "https://graph.microsoft.com/v1.0/applications/$aad_obj_id/addPassword"   )
echo $aad_app_credentials_res
aad_app_secret=$( echo $aad_app_credentials_res | jq -r '.secretText' )
echo $aad_app_secret
