const axios = require("axios");

/**
 * Handler that will be called during the execution of a PostLogin flow.
 *
 * @param {Event} event - Details about the user and the context in which they are logging in.
 * @param {PostLoginAPI} api - Interface whose methods can be used to change the behavior of the login.
 */
exports.onExecutePostLogin = async (event, api) => {
  if (event.stats.logins_count === 1) {
    const response = await axios.post("<ngrok-url>/api/users", {
      externalId: event.user.user_id,
      email: event.user.email,
      connectionName: event.connection.name,
    });

    const userId = response.data.userId;
    api.accessToken.setCustomClaim("userId", userId);
    api.user.setAppMetadata("db_userId", userId);
  } else {
    const userId = event.user.app_metadata["db_userId"];
    api.accessToken.setCustomClaim("userId", userId);
  }
};
