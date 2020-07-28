context("Connectivity and DataIO tests");
accesstoken <- ""

test_that("Test availability", {

  configuration <- dataapi_build_configuration()
  is_available <- dataapi_isavailable(configuration)
  expect_true(is_available)

})

test_that("Test login", {

  configuration <- dataapi_build_configuration()
  login_result <- dataapi_login(configuration, username = "", password = "")
  expect_gt(nchar(login_result$AccessToken), 0)
})

test_that("Test create, get and delete", {

  configuration <- dataapi_build_configuration()
  test_object <- list(Name = "Jan", Age = 29, Height = 1.82, Hobbies = list("Medicine", "Inline skating", "Bioinformatics"), Address = list(Street = "Coding Alley", PostalCode = 1234))
  object_id <- dataapi_insert(configuration, accesstoken, "RTestObjects", test_object)
  expect_gt(nchar(object_id), 0)
  get_object <- dataapi_get(configuration, accesstoken, "RTestObjects", object_id)
  expect_equal(get_object$Name, test_object$Name)
  expect_equal(get_object$Age, test_object$Age)
  expect_equal(get_object$Height, test_object$Height)
  is_deleted <- dataapi_delete(configuration, accesstoken, "RTestObjects", object_id)
  expect_true(is_deleted)
})

test_that("Test get many", {
  configuration <- dataapi_build_configuration();
  limit <- 2;
  items <- dataapi_getmany(configuration, accesstoken, "DataTag", "", limit)
  expect_gte(length(items), limit)
})

test_that("Test get subscribed objects", {
  configuration <- dataapi_build_configuration()
  subscription_id <- dataapi_subscribe(configuration, accesstoken, datatype = "RTestObjects", modificationTypes = list("Created"))
  expect_gt(nchar(subscription_id), 0)
  test_object <- list(Name = "Jan", Age = 29, Height = 1.82, Hobbies = list("Medicine", "Inline skating", "Bioinformatics"), Address = list(Street = "Coding Alley", PostalCode = 1234))
  object_id <- dataapi_insert(configuration, accesstoken, "RTestObjects", test_object)
  notifications <- dataapi_getsubscribedobjects(configuration, accesstoken)
  expect_gt(length(notifications), 0)
  is_notification_deleted <- dataapi_deletenotification(configuration, accesstoken, notifications[[1]]$Id)
  expect_true(is_notification_deleted)
  is_unsubscribed <- dataapi_unsubscribe(configuration, accesstoken, subscription_id)
  expect_true(is_unsubscribed)
  dataapi_delete(configuration, accesstoken, "RTestObjects", object_id)
})

test_that("Test data blob upload", {
  configuration <- dataapi_build_configuration()
  data_blob <- list("Id" = "RUnitTestDataBlob", "Data" = "UmVzZXJ2ZWQ=", "Filename" = "myTestFile.txt")
  object_id <- dataapi_insert(configuration, accesstoken, "DataBlob", data_blob)
  expect_gt(nchar(object_id), 0)
  dataapi_delete(configuration, accesstoken, "DataBlob", object_id)
})

test_that("Test adding to data project", {
  dataprojectid <- "RClientUnitTest"
  datatype <- "RTibble"
  dataid <- "424fd3d5-2d53-4eed-897d-232dac67d588"
  configuration <- dataapi_build_configuration()
  initials <- "jdoe"
  filename <- "RTibble_424fd3d5-2d53-4eed-897d-232dac67d588.json"
  dataapi_addtodataproject(configuration, accesstoken, dataprojectid, datatype, dataid, initials, filename)
  expect_true(TRUE)
})
