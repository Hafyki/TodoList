FROM node:20-alpine3.22

WORKDIR /app

COPY package.json .

COPY package-lock.json .

RUN npm install

COPY . .
RUN npm i -D @ui5/cli
RUN npx ui5 use OpenUI5
RUN npx ui5 add themelib_sap_horizon sap.f sap.m sap.ui.core sap.ui.fl sap.ui.mdc sap.ui.table

EXPOSE 8080

CMD ["npx","ui5","serve","--port","8080","--accept-remote-connections"]

