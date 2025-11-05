FROM node:20-alpine3.22

WORKDIR /app

COPY package.json package-lock.json /app/

RUN npm install

RUN npm set @sap:registry=https://registry.npmjs.org

COPY . .

EXPOSE 8080

#CMD ["npm", "start"]
CMD ["npx","ui5","serve","--port","8080","--accept-remote-connections"]

