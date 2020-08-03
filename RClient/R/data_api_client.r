#' @export
dataapi_build_configuration <- function(server_address = "", server_port = 443){
  return( list(server_address = server_address, server_port = server_port) );
}

#' @export
dataapi_isavailable <- function(configuration)
{
	url <- build_dataapi_url(configuration,"api/servicestatus/ping");
	r <- run_get_request(url);
	return( r$status_code == 200 );
}

#' @export
dataapi_register <- function(configuration, username, firstname, lastname, email, password)
{
	url <- build_dataapi_url(configuration, "api/account/register");
	body <- list(Username = username, FirstName = firstname, LastName = lastname, Email = email, Password = password);
	r <- run_post_request(url, body = body);

	return( r$status_code == 200 );
}

#' @export
dataapi_login <- function(configuration, username, password)
{
	url <- build_dataapi_url(configuration, "api/account/login");
	body <- list(Username = username, Password = password);

	r <- run_post_request(url, body = body);
	return( httr::content(r, "parsed") );
}

#' @export
dataapi_logout <- function(configuration, accesstoken)
{
	return( TRUE );
}

#' @export
dataapi_changepassword <- function(configuration, accesstoken, username, password)
{
	url <- build_dataapi_url(configuration, "api/account/changepassword");
	body <- list(Username = username, Password = password);

	r <- run_post_request(url, body, accesstoken);
	return( r$status_code == 200 );
}

#' @export
dataapi_deleteuser <- function(configuration, accesstoken, username)
{
	url <- build_dataapi_url(configuration, "api/account/deleteuser", list(username = username));
	r <- run_delete_request(url, accesstoken);
	return( r$status_code == 200 );
}

#' @export
dataapi_listcollections <- function(configuration, accesstoken, include_hidden = FALSE)
{
	url <- build_dataapi_url(configuration, "api/dataio/listcollections", list(includeHidden=include_hidden));
	r <- run_get_request(url, accesstoken);
	return( httr::content(r, "parsed") );
}

#' @export
dataapi_listcollectionnames <- function(configuration, accesstoken, include_hidden = FALSE)
{
	url <- build_dataapi_url(configuration, "api/dataio/listcollectionnames", list(includeHidden=include_hidden));
	r <- run_get_request(url, accesstoken);
	return( httr::content(r, "parsed") );
}

#' @export
dataapi_getcollectioninformation <- function(configuration, accesstoken, collectionname)
{
	url <- build_dataapi_url(configuration, "api/dataio/getcollectioninformation", list(collectionName=collectionname));
	r <- run_get_request(url, accesstoken);
	return( httr::content(r, "parsed") );
}

#' @export
dataapi_getcollectionpermissions <- function(configuration, accesstoken, collectionname)
{
	url <- build_dataapi_url(configuration, "api/dataio/getcollectionpermissions", list(collectionName=collectionname));
	r <- run_get_request(url, accesstoken);
	return( httr::content(r, "parsed") );
}

#' @export
dataapi_exists <- function(configuration, accesstoken, datatype, id)
{
	url <- build_dataapi_url(configuration, "api/dataio/exists", list(dataType = datatype, id = id));
	r <- run_get_request(url, accesstoken);
	return( r$status_code == 200 );
}

#' @export
dataapi_get <- function(configuration, accesstoken, datatype, id)
{
	url <- build_dataapi_url(configuration, "api/dataio/get", list(dataType = datatype, id = id));
	r <- run_get_request(url, accesstoken);
	if(r$status_code != 200)
	{
	  stop(paste("Could not find data of type ", datatype, " and ID '", id, "'", sep=""));
	}
	return( httr::content(r, "parsed") );
}

#' @export
dataapi_getmany <- function(configuration, accesstoken, datatype, query, limit = NULL)
{
	url <- build_dataapi_url(configuration, "api/dataio/getmany", list(dataType = datatype, whereArguments = query, limit = limit));
	r <- run_get_request(url, accesstoken);
	if(r$status_code != 200)
	{
	  stop(paste("Could not get data of type ", datatype, sep=""));
	}
	search_result <- read_lines(httr::content(r, "text"))
	return( search_result );
}

#' @export
dataapi_search <- function(configuration, accesstoken, query, result_format = "Json")
{
	url <- build_dataapi_url(configuration, "api/dataio/search");
	body <- list(Query = query, Format = result_format);
	r <- run_post_request(url, body, accesstoken);
	if(r$status_code != 200)
	{
	  stop(paste("Search with query '", query, "' resulted in an error ", r$status_code, ": ", httr::content(r), sep=""));
	}
	search_result <- read_lines(httr::content(r, "text"));
	return( search_result );
}

#' @export
dataapi_insert <- function(configuration, accesstoken, datatype, obj, id = NULL)
{
	url <- build_dataapi_url(configuration, "api/dataio/submit");
	body <- list(DataType = datatype, Id = id, Overwrite = FALSE, Data = obj);
	r <- run_post_request(url, body, accesstoken);
	if(r$status_code != 200)
	{
	  stop(paste("Could not store data of type ", datatype, ": ", httr::content(r), sep=""));
	}
	return( httr::content(r, "text", encoding = "UTF-8") );
}

#' @export
dataapi_replace <- function(configuration, accesstoken, datatype, obj, id = NULL)
{
	url <- build_dataapi_url(configuration, "api/dataio/submit");
	body <- list(DataType = datatype, Id = id, Overwrite = TRUE, Data = obj);
	r <- run_post_request(url, body, accesstoken);
	if(r$status_code != 200)
	{
	  stop(paste("Could not store data of type ", datatype, ": ", httr::content(r), sep=""));
	}
	return( httr::content(r, "text", encoding = "UTF-8") );
}

#' @export
dataapi_delete <- function(configuration, accesstoken, datatype, id)
{
	url <- build_dataapi_url(configuration, "api/dataio/delete", list(dataType = datatype, id = id));
	r <- run_delete_request(url, accesstoken);
	return( r$status_code == 200 );
}

#' @export
dataapi_subscribe <- function(configuration, accesstoken, datatype, modificationTypes, filter = "")
{
	url <- build_dataapi_url(configuration, "api/subscription/subscribe");
	body <- list(DataType = datatype, ModificationTypes = modificationTypes, Filter = filter);
	r <- run_post_request(url, body, accesstoken);
	return( httr::content(r, "text", encoding = "UTF-8") );
}

#' @export
dataapi_unsubscribe <- function(configuration, accesstoken, subscriptionid)
{
	url <- build_dataapi_url(configuration, "api/subscription/unsubscribe", list(id=subscriptionid));
	r <- run_get_request(url, accesstoken);
	return( r$status_code == 200 );
}

#' @export
dataapi_unsubscribeall <- function(configuration, accesstoken, datatype)
{
	url <- build_dataapi_url(configuration, "api/subscription/unsubscribeall", list(dataType=datatype));
	r <- run_get_request(url, accesstoken);
	return( r$status_code == 200 );
}

#' @export
dataapi_getsubscriptions <- function(configuration, accesstoken, datatype = NULL)
{
	url <- build_dataapi_url(configuration, "api/subscription/getsubscriptions", list(dataType=datatype));
	r <- run_get_request(url, accesstoken);
	subscriptions <- read_lines(httr::content(r, "text"))
	return( subscriptions );
}

#' @export
dataapi_getsubscribedobjects <- function(configuration, accesstoken, datatype = NULL)
{
	url <- build_dataapi_url(configuration, "api/subscription/getsubscribedobjects", list(dataType=datatype));
	r <- run_get_request(url, accesstoken);
	notifications <- read_lines(httr::content(r, "text"))
	return( notifications  );
}

#' @export
dataapi_deletenotification <- function(configuration, accesstoken, notificationid)
{
	url <- build_dataapi_url(configuration, "api/subscription/deletenotification", list(notificationId=notificationid));
	r <- run_delete_request(url, accesstoken);
	return( r$status_code == 200 );
}

#' @export
dataapi_reportto <- function(configuration, accesstoken, recipient, datatype, id)
{
	url <- build_dataapi_url(configuration, "api/subscription/reportto", list(recipient=recipient, dataType=datatype, id=id));
	r <- run_get_request(url, accesstoken);
	return( r$status_code == 200 );
}

#' @export
dataapi_validatevalidator <- function(configuration, accesstoken, datatype, validator)
{
	url <- build_dataapi_url(configuration, "api/validator/validate");
	body <- list(ValidatorDefinition=list(DataType = datatype, ValidatorType = "TextRules", Ruleset = validator));
	r <- run_post_request(url, body, accesstoken);
	return( httr::content(r, "parsed") );
}

#' @export
dataapi_submitvalidator <- function(configuration, accesstoken, datatype, validator)
{
	url <- build_dataapi_url(configuration, "api/validator/submit");
	body <- list(ValidatorDefinition=list(DataType = datatype, ValidatorType = "TextRules", Ruleset = validator));
	r <- run_post_request(url, body, accesstoken);
	return( r$status_code == 200 );
}

#' @export
dataapi_getvalidator <- function(configuration, accesstoken, validatorid)
{
	url <- build_dataapi_url(configuration, "api/validator/get", list(validatorId=validatorid));
	r <- run_get_request(url, accesstoken);
	return( httr::content(r, "parsed") );
}

#' @export
dataapi_listvalidators <- function(configuration, accesstoken, collectionname = NULL)
{
	url <- build_dataapi_url(configuration, "api/validator/getall", list(collectionName=collectionname));
	r <- run_get_request(url, accesstoken);
	return( httr::content(r, "parsed") );
}

#' @export
dataapi_approvevalidator <- function(configuration, accesstoken, validatorid)
{
	url <- build_dataapi_url(configuration, "api/validator/approve", list(validatorId=validatorid));
	r <- run_get_request(url, accesstoken);
	return( r$status_code == 200 );
}

#' @export
dataapi_deletevalidator <- function(configuration, accesstoken, validatorid)
{
	url <- build_dataapi_url(configuration, "api/validator/delete", list(validatorId=validatorid));
	r <- run_delete_request(url, accesstoken);
	return( r$status_code == 200 );
}

#' @export
dataapi_createview <- function(configuration, accesstoken, query, viewid = NULL)
{
	url <- build_dataapi_url(configuration, "api/view/create");
	body <- list(Query=query, ViewId=viewid);
	r <- run_post_request(url, body, accesstoken);
	return( httr::content(r, "parsed") );
}

#' @export
dataapi_getview <- function(configuration, accesstoken, viewid, result_format = "Json")
{
	url <- build_dataapi_url(configuration, "api/view/get", list(viewId=viewid, resultFormat=result_format));
	r <- run_get_request(url, accesstoken);
	search_result <- read_lines(httr::content(r, "text"))
	return( search_result  );
}

#' @export
dataapi_deleteview <- function(configuration, accesstoken, viewid)
{
	url <- build_dataapi_url(configuration, "api/view/delete", list(viewId=viewid));
	r <- run_delete_request(url, accesstoken);
	return( r$status_code == 200 );
}

#' @export
dataapi_addtodataproject <- function(configuration, accesstoken, dataprojectid, datatype, dataid, initials, filename = NULL)
{
  upload_id <- uuid::UUIDgenerate();
  current_utc_time <- as.POSIXlt(Sys.time(), tz = "UTC")
  data_project_upload_info <- list(Id = upload_id,
                                   UploaderInitials = initials,
                                   UploadTimestamp = format(current_utc_time, "%Y-%m-%dT%H:%M:%SZ"),
                                   DataProjectId = dataprojectid,
                                   Filename = filename,
                                   RawData = list(
                                     DataType = datatype,
                                     Id = dataid
                                   ),
                                   DerivedData = c()
                               )
  dataapi::dataapi_insert(configuration, accesstoken, 'DataProjectUploadInfo', data_project_upload_info, upload_id)
  return( upload_id );
}

build_dataapi_url <- function(configuration, path, query = list())
{
	full_path <- paste("https://", configuration$server_address, ":", configuration$server_port, "/", path, sep="");
	if(length(query) > 0)
	{
		query_str <- "";
		for(parameterName in ls(query))
		{
			parameterValue <- query[[parameterName]];
			if(!is.null(parameterValue)) {
			  parameterValue <- utils::URLencode(toString(parameterValue));
			  if(query_str != ""){
			    query_str <- paste(query_str, "&", sep="");
			  }
			  query_str <- paste(query_str, paste(parameterName, parameterValue, sep="="), sep = "");
			}
		}
		full_path <- paste(full_path, "?", query_str, sep="");
	}
	return( full_path );
}

run_get_request <- function(url, accesstoken = NULL)
{
  if(is.null(accesstoken))
  {
    return( httr::GET(url));
  }
  else
  {
    return( httr::GET(url, httr::add_headers(Authorization = paste("Bearer", accesstoken, sep = " "))) );
  }
}

run_delete_request <- function(url, accesstoken = NULL)
{
  if(is.null(accesstoken))
  {
    return( httr:: DELETE(url));
  }
  else
  {
    return( httr::DELETE(url, httr::add_headers(Authorization = paste("Bearer", accesstoken, sep = " "))) );
  }
}

run_post_request <- function(url, body, accesstoken = NULL)
{
  if(is.null(accesstoken))
  {
    return( httr::POST(url, body = rjson::toJSON(body), httr::add_headers(`Content-Type` = "application/json")));
  }
  else
  {
    return( httr::POST(url, body = rjson::toJSON(body), httr::add_headers(`Content-Type` = "application/json", Authorization = paste("Bearer", accesstoken, sep = " "))) );
  }
}

read_lines <- function(content)
{
  lines <- strsplit(content, "\n")[[1]]
  search_result <- list();
  item_index <- 0;
  for(line in lines)
  {
    item_index <- item_index + 1;
    parsed_item <- rjson::fromJSON(line);
    search_result[[item_index]] <- parsed_item;
  }
  return(search_result)
}
